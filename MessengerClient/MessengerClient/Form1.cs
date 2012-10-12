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
        private System.Net.IPAddress ipAddress;
        private int port;

        Form2 form2;

        private delegate void xThread_Message(string message);

        System.Threading.Thread UpdateThread;

        public Messenger()
        {
            InitializeComponent();

            form2 = new Form2();
        }

        private void Messenger_Load(object sender, EventArgs e)
        {
            this.Show();
            GetInformation();

            StreamWriter writer = new StreamWriter(tcpClient.GetStream());
            writer.WriteLine("Mark");
            writer.Flush();

            textBox.Text = "You have been connected.";

            UpdateThread = new System.Threading.Thread(new System.Threading.ThreadStart(UpdateClient));
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
                string output = reader.ReadLine();
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

        private void writeMessage(string message)
        {
            if (!textBox.InvokeRequired)
            { textBox.Text += message + '\n'; }
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
            { userTextBox.Text = usersList; }
            else
            {
                xThread_Message d = new xThread_Message(usersChanged);
                object[] o = new object[1];
                o[0] = usersList;
                Invoke(d, o);
            }
        }

        private void Messenger_FormClosing(object sender, FormClosingEventArgs e)
        {
            UpdateThread.Abort();
            tcpClient.Close();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (tcpClient.Connected && chatBox.Text != "")
            {
                StreamWriter writer = new StreamWriter(tcpClient.GetStream());
                writer.WriteLine(chatBox.Text);
                writer.Flush();
                chatBox.Text = "";
            }
            
        }
    }
}
