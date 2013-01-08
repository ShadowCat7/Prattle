﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.IO;

namespace InstantMessenger
{
    public partial class ChatServer : Form
    {
        private System.Net.Sockets.TcpListener chatServer;
        private System.Net.IPAddress ipAddress;
        private System.Net.IPAddress extIP;
        private int port;

        private List<Communicator> Users;

        private string Command;

        private Thread UpdateThread;

        private delegate void xThread_Message(string message);
        private delegate void xThread_UserList();

        public ChatServer()
        {
            InitializeComponent();

            userTextBox.Text = "Users:\nNone";

            AcceptButton = sendButton;

            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            ipAddress = System.Net.IPAddress.Parse(localIP);
            port = 24658;

            WebRequest request = WebRequest.Create("http://www.jsonip.com/");
            WebResponse response = request.GetResponse(); //TODO
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string webString = reader.ReadToEnd();
            string ipString = "";
            for (int i = 0; i < webString.Length; i++)
            {
                if (webString[i] == ':')
                {
                    i += 2;
                    while (webString[i] != '"')
                    {
                        ipString += webString[i];
                        i++;
                    }
                    i = webString.Length;
                }
            }
            extIP = System.Net.IPAddress.Parse(ipString);

            chatServer = new System.Net.Sockets.TcpListener(ipAddress, port);
            chatServer.Start();

            Users = new List<Communicator>(0);

            Command = "Nothing.";

            List<string> tempString = new List<string>();
            tempString.Add("/%text%/");
            tempString.Add("Chat server has been started.%/\t*LAN IP Address: " + ipAddress.ToString()
                + "%/\tExternal IP Address: " + extIP.ToString() + "%/");

            SendMessage(tempString);

            UpdateThread = new Thread(new ThreadStart(UpdateServer));
            UpdateThread.IsBackground = true;
            UpdateThread.Start();
        }

        public void UpdateServer()
        {
            while (true)
            {
                if (chatServer.Pending())
                {
                    System.Net.Sockets.TcpClient chatConnection =
                        chatServer.AcceptTcpClient();

                    Users.Add(new Communicator(chatConnection));
                    if (checkBanList(Users[Users.Count - 1].ipAddress))
                    {
                        Users[Users.Count - 1].writer.WriteLine("/%text%/");
                        Users[Users.Count - 1].writer.WriteLine("You have been banned from this server.%/");
                        Users[Users.Count - 1].writer.Flush();
                        Users[Users.Count - 1].client.Close();
                        Users.RemoveAt(Users.Count - 1);
                    }
                    else
                    {
                        usersChanged();

                        List<string> tempString = new List<string>();
                        tempString.Add("/%text%/");
                        tempString.Add("\t" + Users[Users.Count - 1].Name +
                            " has joined the chat.%/");

                        SendMessage(tempString);
                    }
                }

                for (int i = 0; i < Users.Count; i++)
                {
                    if (Users[i].Message != "" && Users[i].Message != null)
                    {
                        List<string> tempString = new List<string>();
                        tempString.Add("/%text%/");
                        tempString.Add(Users[i].Name + " : " + Users[i].Message + "%/");

                        SendMessage(tempString);
                        Users[i].Message = "";
                    }

                    if (!Users[i].client.Connected)
                    {
                        List<string> tempString = new List<string>();
                        tempString.Add("/%text%/");
                        if (Users[i].Name == null)
                        { Users[i].Name = "unknown_user"; }
                        tempString.Add(Users[i].Name + " has disconnected.%/");
                        SendMessage(tempString);
                        try
                        { Users.Remove(Users[i]); }
                        catch (ArgumentOutOfRangeException) { }
                        usersChanged();
                    }
                }
            }
        }

        void Commanded()
        {
            string tempCommand = "";
            tempCommand = lowerToCaps(Command);

            bool isCommandValid = false;

            if (tempCommand == "QUIT")
            { Close(); }
            else if (tempCommand == "HELP")
            {
                writeMessage("Server Commands:\n\tSay: Text written after 'say' will be sent to all users as a message." +
                    "\n\tKick: The user whose name is written after 'kick' will be forcibly removed." +
                    "\n\tBan: The user whose name is written after 'ban' will be permanently removed." +
                    "\n\tPardon: The IP address that is written after 'pardon' will no longer be banned." +
                    "\n\tQuit: Closes the server.\n");
                isCommandValid = true;
            }
            else if (tempCommand == "BANNED IPS")
            {
                string banList = getBanList();
                if (banList == "")
                { textBox.Text += "No users are banned.\n"; }
                else
                { textBox.Text += getBanList(); }
                isCommandValid = true;
            }
            else
            {
                string tempString = "";
                for (int i = 0; i < tempCommand.Length; i++)
                {
                    tempString += tempCommand[i];
                    if (tempString == "SAY ")
                    {
                        tempString = "";
                        for (int j = 0; j < Command.Length - 4; j++)
                        { tempString += Command[j + 4]; }


                        List<string> tempListString = new List<string>();
                        tempListString.Add("/%text%/");
                        tempListString.Add("Server : " + tempString + "%/");
                        SendMessage(tempListString);
                        isCommandValid = true;
                    }
                    else if (tempString == "PARDON ")
                    {
                        tempString = "";
                        for (int j = 0; j < Command.Length - 7; j++)
                        { tempString += Command[j + 7]; }

                        tempString = lowerToCaps(tempString);

                        if (removeFromBanList(tempString))
                        {
                            List<string> tempList = new List<string>();
                            tempList.Add("/%text%/");
                            tempList.Add(tempString + " has been pardoned.%/");
                            SendMessage(tempList);
                        }
                        else
                        { textBox.Text += "That IP Address is not banned.\n"; }
                        isCommandValid = true;
                    }
                    else if (tempString == "BAN ")
                    {
                        tempString = "";
                        for (int j = 0; j < Command.Length - 4; j++)
                        { tempString += Command[j + 4]; }

                        tempString = lowerToCaps(tempString);

                        bool foundName = false;
                        for (int j = 0; j < Users.Count; j++)
                        {
                            if (tempString == lowerToCaps(Users[j].Name))
                            {
                                List<string> tempList = new List<string>();
                                tempList.Add("/%text%/");
                                tempList.Add(Users[j].Name + " (" + Users[j].ipAddress + ") has been banned.%/");
                                Users[j].writer.WriteLine("/%text%/");
                                Users[j].writer.WriteLine("You have been banned.%/");
                                Users[j].writer.Flush();
                                addToBanList(Users[j].ipAddress);
                                Users[j].ReceiveMessage.Abort();
                                Users[j].client.Close();
                                try
                                { Users.Remove(Users[j]); }
                                catch (IndexOutOfRangeException) { }
                                usersChanged();
                                SendMessage(tempList);
                                foundName = true;
                            }
                        }
                        if (!foundName)
                        { textBox.Text += "That is not any user's name.\n"; }
                        isCommandValid = true;
                    }
                    else if (tempString == "KICK ")
                    {
                        tempString = "";
                        for (int j = 0; j < Command.Length - 5; j++)
                        { tempString += Command[j + 5]; }

                        tempString = lowerToCaps(tempString);

                        bool foundName = false;
                        for (int j = 0; j < Users.Count; j++)
                        {
                            if (tempString == lowerToCaps(Users[j].Name))
                            {
                                List<string> tempList = new List<string>();
                                tempList.Add("/%text%/");
                                tempList.Add(Users[j].Name + " (" + Users[j].ipAddress + ") has been kicked.%/");
                                Users[j].writer.WriteLine("/%text%/");
                                Users[j].writer.WriteLine("You have been kicked.%/");
                                Users[j].writer.Flush();
                                Users[j].ReceiveMessage.Abort();
                                Users[j].client.Close();
                                try
                                { Users.Remove(Users[j]); }
                                catch (ArgumentOutOfRangeException) { }
                                usersChanged();
                                SendMessage(tempList);
                                foundName = true;
                            }
                        }
                        if (!foundName)
                        { textBox.Text += "That is not any user's name.\n"; }
                        isCommandValid = true;
                    }
                }
            }
            if (!isCommandValid)
            { textBox.Text += "Not a valid command. Type help for information on commands.\n"; }
        }

        private string lowerToCaps(string input)
        {
            string tempString = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] >= 'a' && input[i] <= 'z')
                { tempString += (char)(input[i] - 32); }
                else
                { tempString += input[i]; }
            }
            return tempString;
        }

        void writeMessage(string message)
        {
            if (!textBox.InvokeRequired)
            {
                textBox.Text += message;
                textBox.SelectionStart = textBox.Text.Length;
                textBox.ScrollToCaret();
            }
            else
            {
                xThread_Message d = new xThread_Message(writeMessage);
                object[] o = new object[1];
                o[0] = message;
                Invoke(d, o);
            }
        }

        void SendMessage(List<string> message)
        {
            for (int i = 0; i < Users.Count; i++)
            {
                try
                {
                    Users[i].writer.WriteLine(message[0]);
                    Users[i].writer.WriteLine(message[1]);
                    Users[i].writer.Flush();
                }
                catch { }
            }

            if (message[0] == "/%text%/")
            {
                string tempString = "";
                if (message.Count > 0)
                { tempString = convertToReturns(message[1]); }
                writeMessage(tempString);
            }
        }

        private void usersChanged()
        {
            if (!userTextBox.InvokeRequired)
            {
                List<string> tempString = new List<string>();
                tempString.Add("/%users%/");
                tempString.Add("Online:%/");

                for (int i = 0; i < Users.Count; i++)
                { tempString[1] += Users[i].Name + "%/"; }

                if (Users.Count == 0)
                { tempString[1] += "None"; }

                string tempUsernames = "";
                if (tempString.Count > 0)
                { tempUsernames = convertToReturns(tempString[1]); }

                userTextBox.Text = tempUsernames;

                SendMessage(tempString);
            }
            else
            {
                xThread_UserList d = new xThread_UserList(usersChanged);
                Invoke(d);
            }
        }

        private void addToBanList(string address)
        {
            if (!File.Exists("BanList"))
            { File.Create("BanList").Close(); }

            StreamWriter writer = new StreamWriter("BanList", true);
            writer.WriteLine(address);
            writer.Flush();
            writer.Close();
        }
        private bool checkBanList(string address)
        {
            if (File.Exists("BanList"))
            {
                StreamReader reader = new StreamReader("BanList");
                while (!reader.EndOfStream)
                {
                    if (reader.ReadLine() == address)
                    { return true; }
                }
                reader.Close();
            }
            return false;
        }
        private bool removeFromBanList(string address)
        {
            if (File.Exists("BanList"))
            {
                StreamReader reader = new StreamReader("BanList");
                List<string> bannedIPs = new List<string>();
                while (!reader.EndOfStream)
                { bannedIPs.Add(reader.ReadLine()); }
                reader.Close();
                for (int i = 0; i < bannedIPs.Count; i++)
                {
                    if (address == bannedIPs[i])
                    { bannedIPs.RemoveAt(i); }
                }
                File.Delete("BanList");
                File.Create("BanList").Close();
                StreamWriter writer = new StreamWriter("BanList");
                for (int i = 0; i < bannedIPs.Count; i++)
                { writer.WriteLine(bannedIPs[i]); }
                writer.Close();

                return true;
            }
            return false;
        }
        private string getBanList()
        {
            if (File.Exists("BanList"))
            {
                StreamReader reader = new StreamReader("BanList");
                string tempString = reader.ReadToEnd();
                reader.Close();
                return tempString;
            }
            return "";
        }

        private string convertToReturns(string message)
        {
            string tempString = "";
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == '%' && message[i + 1] == '/')
                {
                    i++;
                    tempString += '\r';
                }
                else
                { tempString += message[i]; }
            }
            return tempString;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        { UpdateThread.Abort(); }

        private void sendButton_Click(object sender, EventArgs e)
        { messageReady(); }

        private void messageReady()
        {
            Command = chatBox.Text;
            chatBox.Text = "";
            Commanded();
            chatBox.Focus();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        { this.Close(); }

        private void ChatServer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            { messageReady(); }
        }

        private void iPAddressToolStripMenuItem_Click(object sender, EventArgs e)
        { MessageBox.Show("LAN IP Address: " + ipAddress.ToString() + "\nExternal IP Address: " + extIP.ToString()); }
    }
}
