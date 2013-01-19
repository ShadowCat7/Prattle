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
    public partial class Form1 : Form
    {
        private string command;

        private Server server;

        private delegate void xThread_Message(string message);
        private delegate void xThread_UserList();

        public Form1()
        {
            InitializeComponent();
            AcceptButton = sendButton;

            userTextBox.Text = "Users:\nNone";
            
            server = new Server(writeMessage, usersChanged);

            command = "Nothing.";

            writeMessage("Chat server has been started.%/\tLAN IP Address: " + server.getLocalIP().ToString()
                + "%/\tExternal IP Address: " + server.getExtIP().ToString() + 
                "%/\tDomain: " + server.getDomainAddress() + "%/%/");
        }

        void commanded()
        {
            string tempCommand = "";
            tempCommand = lowerToCaps(command);

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
                string banList = FileManager.getBanList();
                if (banList == "")
                { writeMessage("No users are banned.\n"); }
                else
                { writeMessage("\tBanned IP Addresses:%/" + banList + "%/"); }
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
                        for (int j = 0; j < command.Length - 4; j++)
                        { tempString += command[j + 4]; }


                        List<string> tempListString = new List<string>();
                        tempListString.Add("/%text%/");
                        tempListString.Add("Server : " + tempString + "%/");
                        server.sendMessage(tempListString);
                        isCommandValid = true;
                    }
                    else if (tempString == "PARDON ")
                    {
                        tempString = "";
                        for (int j = 0; j < command.Length - 7; j++)
                        { tempString += command[j + 7]; }

                        tempString = lowerToCaps(tempString);

                        if (FileManager.removeFromBanList(tempString))
                        {
                            List<string> tempList = new List<string>();
                            tempList.Add("/%text%/");
                            tempList.Add(tempString + " has been pardoned.%/");
                            server.sendMessage(tempList);
                        }
                        else
                        { textBox.Text += "That IP Address is not banned.\n"; }
                        isCommandValid = true;
                    }
                    else if (tempString == "BAN ")
                    {
                        tempString = "";
                        for (int j = 0; j < command.Length - 4; j++)
                        { tempString += command[j + 4]; }

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
                                writeMessage("(" + tempIP + ") ");
                                FileManager.addToBanList(tempIP);
                                usersChanged();
                                server.sendMessage(tempList);
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
                        for (int j = 0; j < command.Length - 5; j++)
                        { tempString += command[j + 5]; }

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
                                server.sendMessage(tempList);
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
                message = convertToReturns(message);
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

                server.sendMessage(tempString);
            }
            else
            {
                xThread_UserList d = new xThread_UserList(usersChanged);
                Invoke(d);
            }
        }

        private string convertToReturns(string message)
        {
            string tempString = "";
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == '%' && message[i + 1] == '/')
                {
                    i++;
                    tempString += '\n';
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
            command = chatBox.Text;
            chatBox.Text = "";
            commanded();
            chatBox.Focus();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        { this.Close(); }

        private void form1_KeyDown(object sender, KeyEventArgs e)
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
