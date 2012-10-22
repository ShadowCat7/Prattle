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
    public partial class Form2 : Form
    {
        private System.Net.IPAddress serverIP;
        private int port;

        public System.Net.Sockets.TcpClient checkClient;

        public Form2()
        {
            InitializeComponent();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            AcceptButton = button1;

            //todo
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (System.Net.IPAddress.TryParse(ipTextBox.Text, out serverIP) && portTextBox.Text != "")
            {
                port = Convert.ToInt32(portTextBox.Text);
                checkClient = new System.Net.Sockets.TcpClient();
                try
                { checkClient.Connect(serverIP, port); }
                catch
                { label1.Text = "Not a valid IP Address.\nInput the IP Address of the server:"; }
                if (checkClient.Connected)
                {
                    label1.Text = "Connected!";
                    Close();
                }
            }
            else
            { label1.Text = "Not a valid IP Address.\nInput the IP Address of the server:"; }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            //TODO
        }
    }
}
