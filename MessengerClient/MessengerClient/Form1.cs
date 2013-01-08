using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Prattle
{
    public partial class Prattle : Form
    {
        private string Name;
        private string Message;

        private System.Net.Sockets.TcpClient tcpClient;
        private List<string> ipAddresses;

        Form2 form2;

        private delegate void xThread_Message(string message);
        private delegate void xGetForm();

        System.Threading.Thread UpdateThread;

        public Prattle()
        {
            InitializeComponent();
            
            form2 = new Form2();
            ipAddresses = new List<string>();
        }

        private void Messenger_Load(object sender, EventArgs e)
        {
            this.Show();
            AcceptButton = sendButton;

            if (!File.Exists("PreviousIPs"))
            { File.Create("PreviousIPs").Close(); }
            StreamReader reader = new StreamReader("PreviousIPs");

            while (!reader.EndOfStream)
            { ipAddresses.Add(reader.ReadLine()); }
            reader.Close();
        }

        public void changeToolStrips()
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

        private void JustConnected()
        {
            textBox.Text += "You have been connected.\n";
            writeMessage("Please type your name.\n");

            UpdateThread = new System.Threading.Thread(new System.Threading.ThreadStart(UpdateClient));
            UpdateThread.IsBackground = true;
            UpdateThread.Start();
        }

        private void GetInformation()
        {
            form2.ShowDialog();
            tcpClient = form2.checkClient;

            if (tcpClient.Connected)
            {
                ipAddresses.Add(form2.serverIP.ToString());

                if (ipAddresses.Count > 7)
                { ipAddresses.RemoveAt(0); }

                changeToolStrips();

                File.Delete("PreviousIPs");
                File.Create("PreviousIPs").Close();

                StreamWriter writer = new StreamWriter("PreviousIPs");

                writer.AutoFlush = true;
                for (int i = 0; i < ipAddresses.Count; i++)
                { writer.WriteLine(ipAddresses[i]); }

                writer.Close();


                JustConnected();
            }
        }

        private void UpdateClient()
        {
            while (true)
            {
                try
                {
                    StreamReader reader;
                    if (tcpClient.Connected)
                    {
                        reader = new StreamReader(tcpClient.GetStream());
                        try
                        {
                            string output = reader.ReadLine();
                            if (reader.EndOfStream)
                            { Disconnect(); }
                            else
                            {
                                if (output != "")
                                {
                                    if (output == "/%text%/")
                                    {
                                        output = reader.ReadLine();
                                        writeMessage(output);
                                    }
                                    if (output == "/%users%/")
                                    {
                                        output = reader.ReadLine();
                                        usersChanged(output);
                                    }
                                    GetAttention();
                                }
                            }
                        }
                        catch (IOException)
                        {
                            Disconnect();
                        }
                    }
                }
                catch (NullReferenceException)
                { }
            }
        }

        private void Disconnect()
        {
            if (tcpClient.Client.Connected)
            {
                tcpClient.Close();
                writeMessage("You are no longer connected.\n");
                usersChanged("Users:%/None");
                UpdateThread.Abort();
            }
        }

        private void writeMessage(string message)
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

        private void usersChanged(string usersList)
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
                    tempString += '\r';
                }
                else
                { tempString += message[i]; }
            }
            return tempString;
        }

        private void Messenger_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Application.Exit();
            //UpdateThread.Abort();
            //tcpClient.Close();
            //this.Close();
            //TODO Why isn't this working?
        }

        private void sendButton_Click(object sender, EventArgs e)
        { messageReady(); }

        private void messageReady()
        {
            try
            {
                if (tcpClient.Connected && chatBox.Text != "")
                {
                    StreamWriter writer = new StreamWriter(tcpClient.GetStream());
                    writer.WriteLine(chatBox.Text);
                    writer.Flush();
                    chatBox.Text = "";
                }
            }
            catch (NullReferenceException)
            { textBox.Text += "You are not connected to a server.\n"; }
            chatBox.Focus();
        }

        private void GetAttention()
        {
            if (!this.InvokeRequired)
            {
                if (!chatBox.Focused)
                {
                    FlashWindow.Flash(this, 1);
                    System.Media.SoundPlayer play = new System.Media.SoundPlayer("alert.wav");
                    play.Play();
                }
            }
            else
            {
                xGetForm d = new xGetForm(GetAttention);
                Invoke(d);
            }
        }

        private void iPAddressToolStripMenuItem_Click(object sender, EventArgs e)
        { GetInformation(); }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        { Disconnect(); }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        { this.Close(); }

        private void Messenger_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            { messageReady(); }
        }
    }
}
