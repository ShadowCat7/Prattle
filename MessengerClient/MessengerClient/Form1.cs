using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Prattle
{
    public partial class Form1 : Form
    {
        Client client;

        private delegate void xThread_Message(string message);
        private delegate void xGetForm();

        public Form1()
        { InitializeComponent(); }

        private void Messenger_Load(object sender, EventArgs e)
        {
            this.Show();
            AcceptButton = sendButton;

            client = new Client(writeMessage, usersChanged);

            FileManager.getConfiguration();
            saveHistoryToolStripMenuItem.Checked = Config.saveChatHistory;

            changeToolStrips(FileManager.getPreviousIps());
        }

        public void changeToolStrips(List<string> ipAddresses)
        {
            try
            {
                toolStripMenuItem1.Text = ipAddresses[ipAddresses.Count - 1];
                toolStripMenuItem2.Text = ipAddresses[ipAddresses.Count - 2];
                toolStripMenuItem3.Text = ipAddresses[ipAddresses.Count - 3];
                toolStripMenuItem4.Text = ipAddresses[ipAddresses.Count - 4];
                toolStripMenuItem5.Text = ipAddresses[ipAddresses.Count - 5];
                toolStripMenuItem6.Text = ipAddresses[ipAddresses.Count - 6];
                toolStripMenuItem7.Text = ipAddresses[ipAddresses.Count - 7];
            }
            catch (ArgumentOutOfRangeException) { }
        }

        public void getInformation()
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();

            if (form2.checkClient.Connected)
            {
                client = new Client(form2.checkClient, writeMessage, usersChanged);
                writeMessage("You have been connected.\n");
                writeMessage("Please type your name.\n");

                FileManager.saveToPreviousIps(form2.address);

                changeToolStrips(FileManager.getPreviousIps());
            }
        }

        public void writeMessage(string message)
        {
            if (!textBox.InvokeRequired)
            {
                message = convertToReturns(message);
                textBox.Text += message;
                textBox.SelectionStart = textBox.Text.Length;
                textBox.ScrollToCaret();
                FileManager.writeToChatHistory(message);
            }
            else
            {
                xThread_Message d = new xThread_Message(writeMessage);
                object[] o = new object[1];
                o[0] = message;
                Invoke(d, o);
            }
        }

        public void usersChanged(string usersList)
        {
            if (!userTextBox.InvokeRequired)
            {
                usersList = convertToReturns(usersList);
                userTextBox.Text = usersList;
            }
            else
            {
                xThread_Message d = new xThread_Message(usersChanged);
                object[] o = new object[1];
                o[0] = usersList;
                Invoke(d, o);
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

        private void messageReady()
        {
            chatBox.Focus();
            if (chatBox.Text != "")
            {
                if (!client.sendMessage(chatBox.Text))
                { writeMessage("You are not connected to a server.\n"); }
                else
                { chatBox.Text = ""; }
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        { messageReady(); }

        private void GetAttention()
        {
            if (!chatBox.Focused)
            {
                FlashWindow.Flash(this, 1);
                System.Media.SoundPlayer play = new System.Media.SoundPlayer("alert.wav");
                play.Play();
            }
        }

        private void iPAddressToolStripMenuItem_Click(object sender, EventArgs e)
        { getInformation(); }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        { client.disconnect(); }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        { this.Close(); }

        private void Messenger_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            { messageReady(); }
            //if (e.Modifiers == Keys.Control && e.Modifiers == Keys.Back)
            //{ } TODO
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        { client.connect(toolStripMenuItem1.Text); }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        { client.connect(toolStripMenuItem2.Text); }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        { client.connect(toolStripMenuItem3.Text); }
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        { client.connect(toolStripMenuItem4.Text); }
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        { client.connect(toolStripMenuItem5.Text); }
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        { client.connect(toolStripMenuItem6.Text); }
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        { client.connect(toolStripMenuItem7.Text); }

        private void clearHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to completely delete your entire chat history?",
                "Prattle Chat History", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            { FileManager.deleteChatHistory(); }            
        }

        private void changeHistorySaveState(object sender, EventArgs e)
        { Config.saveChatHistory = !Config.saveChatHistory; }

        private void showHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO MessageBox.Show();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        { GetAttention(); }

        private void userTextBox_TextChanged(object sender, EventArgs e)
        { GetAttention(); }
    }
}
