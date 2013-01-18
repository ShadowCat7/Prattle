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
    public partial class Form2 : Form
    {
        public System.Net.IPAddress serverIP;

        public System.Net.Sockets.TcpClient checkClient;

        public Form2()
        { InitializeComponent(); }

        private void Form2_Load(object sender, EventArgs e)
        {
            AcceptButton = button1;
            checkClient = new System.Net.Sockets.TcpClient();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (System.Net.IPAddress.TryParse(ipTextBox.Text, out serverIP))
            {
                try
                { checkClient.Connect(serverIP, 24658); }
                catch
                { label1.Text = "No server at that address.\nInput the IP Address of a server:"; }
            }
            else
            {
                try
                { checkClient.Connect(ipTextBox.Text, 24658); }
                catch
                { label1.Text = "No server at that address.\nInput the IP Address of a server:"; }
            }

            if (checkClient.Connected)
            {
                label1.Text = "Connected!";
                Close();
            }
        }
    }
}
