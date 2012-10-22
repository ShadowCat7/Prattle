using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MessengerClient
{
    public partial class Messenger : Form
    {
        private string Name;
        private string Message;

        private System.Net.Sockets.TcpClient tcpClient;
        private List<System.Net.IPAddress> ipAddresses;
        private List<int> ports; //Might come in handy later.

        Form2 form2;

        private delegate void xThread_Message(string message);

        System.Threading.Thread UpdateThread;

        public Messenger()
        {
            InitializeComponent();

            form2 = new Form2();
            ipAddresses = new List<System.Net.IPAddress>();
            ports = new List<int>();
        }

        private void Messenger_Load(object sender, EventArgs e)
        {
            this.Show();
            AcceptButton = sendButton;

            if (!File.Exists("PreviousIPs"))
            { File.Create("PreviousIPs"); }
            StreamReader reader = new StreamReader("PreviousIPs");
            
            string fileInfo = reader.ReadToEnd();
            reader.Close();
            if (fileInfo != "")
            {
                string tempString = "";
                for (int i = 0; i < fileInfo.Length; i++)
                {
                    if (fileInfo[i] == ':')
                    {
                        ipAddresses.Add(System.Net.IPAddress.Parse(tempString));
                        tempString = "";
                    }
                    else if (fileInfo[i] == ';')
                    {
                        ports.Add(Convert.ToInt32(tempString));
                        tempString = "";
                    }
                    else
                    { tempString += fileInfo[i]; }
                }
                this.Show();
            }

            GetInformation();

            JustConnected();
        }

        private void JustConnected()
        {
            StreamWriter writer = new StreamWriter(tcpClient.GetStream());

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
        }

        private void UpdateClient()
        {
            while (true)
            {
                StreamReader reader = new StreamReader(tcpClient.GetStream());
                try
                {
                    string output = reader.ReadLine();
                    if (reader.EndOfStream)
                    {
                        Disconnect();
                    }
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
                        }
                    }
                }
                catch (IOException)
                {
                    Disconnect();
                }
            }
        }

        private void Disconnect()
        {
            tcpClient.Close();
            writeMessage("You are no longer connected.\n");
            usersChanged("Users:%/None");
            UpdateThread.Abort();
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

        private void iPAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetInformation();
            JustConnected();
        }

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
