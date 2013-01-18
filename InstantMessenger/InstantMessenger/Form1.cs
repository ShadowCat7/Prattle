using System;
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
        private string Command;

        private Server server;

        private delegate void xThread_Message(string message);
        private delegate void xThread_UserList();

        public ChatServer()
        {
            InitializeComponent();
            AcceptButton = sendButton;

            userTextBox.Text = "Users:\nNone";
            
            server = new Server(writeMessage, usersChanged);

            Command = "Nothing.";

            writeMessage("Chat server has been started.%/\t*LAN IP Address: " + server.getLocalIP().ToString()
                + "%/\tExternal IP Address: " + server.getExtIP().ToString() + "%/" + server.getDomainAddress());
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
                        server.SendMessage(tempListString);
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
                            server.SendMessage(tempList);
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
                        for (int j = 0; j < server.getUserCount(); j++)
                        {
                            if (tempString == lowerToCaps(server.getUserNames()[j]))
                            {
                                List<string> tempList = new List<string>();
                                tempList.Add("/%text%/");
                                tempList.Add(server.getUserNames()[j] + " has been banned.%/");
                                string tempIP = server.banUser(server.getUserNames()[j]);
                                writeMessage("(" + tempIP + ")");
                                addToBanList(tempIP);
                                usersChanged();
                                server.SendMessage(tempList);
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
                        for (int j = 0; j < server.getUserCount(); j++)
                        {
                            if (tempString == lowerToCaps(server.getUserNames()[j]))
                            {
                                List<string> tempList = new List<string>();
                                tempList.Add("/%text%/");
                                tempList.Add(server.getUserNames()[j] + " has been kicked.%/");
                                server.kickUser(server.getUserNames()[j]);
                                server.SendMessage(tempList);
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

        void usersChanged()
        {
            if (!userTextBox.InvokeRequired)
            {
                List<string> tempString = new List<string>();
                tempString.Add("/%users%/");
                tempString.Add("Online:%/");

                for (int i = 0; i < server.getUserCount(); i++)
                { tempString[1] += server.getUserNames()[i] + "%/"; }

                if (server.getUserCount() == 0)
                { tempString[1] += "None"; }

                string tempUsernames = "";
                if (tempString.Count > 0)
                { tempUsernames = convertToReturns(tempString[1]); }

                userTextBox.Text = tempUsernames;

                server.SendMessage(tempString);
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
        {
            MessageBox.Show("LAN IP Address: " + server.getLocalIP().ToString() + 
            "\nExternal IP Address: " + server.getExtIP().ToString() +
            "\nDomain Name: " + server.getDomainAddress());
        }
    }
}
