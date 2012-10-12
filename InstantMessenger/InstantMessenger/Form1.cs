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
        private System.Net.Sockets.TcpListener chatServer;
        private System.Net.IPAddress ipAddress;
        private int port;

        private List<Communicator> Users;

        private string Command;

        private Thread UpdateThread;

        private delegate void xThread_Message(List<string> message);
        private delegate void xThread_UserList();

        public ChatServer()
        {
            InitializeComponent();

            userTextBox.Text = "Users:\nNone";

            //WebRequest request = WebRequest.Create("http://www.jsonip.com/");
            //WebResponse response = request.GetResponse();
            //StreamReader reader = new StreamReader(response.GetResponseStream());
            //string webString = reader.ReadToEnd();
            //string ipString = "";
            //for (int i = 0; i < webString.Length; i++)
            //{
            //    if (webString[i] == ':')
            //    { 
            //        i += 2;
            //        while (webString[i] != '"')
            //        {
            //            ipString += webString[i];
            //            i++;
            //        }
            //        i = webString.Length;
            //    }
            //}
            //ipAddress = System.Net.IPAddress.Parse(ipString);

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

            chatServer = new System.Net.Sockets.TcpListener(ipAddress, port);
            chatServer.Start();

            Users = new List<Communicator>(0);

            Command = "Nothing.";

            List<string> tempString = new List<string>();
            tempString.Add("/%text%/");
            tempString.Add("Chat server has been started.\n\t*IP Address: " + ipAddress.ToString()
                + " Port: " + port.ToString());

            SendMessage(tempString);

            UpdateThread = new Thread(new ThreadStart(UpdateServer));
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
                    usersChanged();

                    List<string> tempString = new List<string>();
                    tempString.Add("/%text%/");
                    tempString.Add("\t" + Users[Users.Count - 1].Name +
                        " has joined the chat.");

                    SendMessage(tempString);
                }

                for (int i = 0; i < Users.Count; i++)
                {
                    if (Users[i].Message != "" && Users[i].Message != null)
                    {
                        List<string> tempString = new List<string>();
                        tempString.Add("/%text%/");
                        tempString.Add(Users[i].Name + " : " + Users[i].Message);

                        SendMessage(tempString);
                        Users[i].Message = "";

                        if (!Users[i].client.Connected)
                        {
                            Users.Remove(Users[i]);
                            usersChanged();
                        }
                    }
                }
            }
        }

        void Commanded()
        {
            string tempCommand = "";
            for (int i = 0; i < Command.Length; i++)
            {
                if (Command[i] >= 'a' && Command[i] <= 'z')
                { tempCommand += (char)(Command[i] - 32); }
                else
                { tempCommand += Command[i]; }
            }

            if (tempCommand == "HELP")
            {

            }

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
                    tempListString.Add("Server : " + tempString);
                    SendMessage(tempListString);
                }
            }

            if (Command == "QUIT")
            { Close(); }
        }

        void SendMessage(List<string> message)
        {
            if (!textBox.InvokeRequired)
            {
                if (message[0] == "/%text%/")
                { textBox.Text += message[1] + '\n'; }
                for (int i = 0; i < Users.Count; i++)
                {
                    Users[i].writer.WriteLine(message[0]);
                    Users[i].writer.WriteLine(message[1]);
                    try
                    {
                        Users[i].writer.Flush();
                    }
                    catch (Exception IOException)
                    {
                        Users[i].client.Close();
                        Users.Remove(Users[i]);
                        usersChanged();
                    }
                }
            }
            else
            {
                xThread_Message d = new xThread_Message(SendMessage);
                object[] o = new object[1];
                o[0] = message;
                Invoke(d, o);
            }
        }

        private void usersChanged()
        {
            if (!userTextBox.InvokeRequired)
            {
                List<string> tempString = new List<string>();
                tempString.Add("/%users%/");
                tempString.Add("Online:");

                for (int i = 0; i < Users.Count; i++)
                { tempString[1] += Users[i].Name + "%/"; }

                if (Users.Count == 0)
                { tempString[1] = "None"; }

                userTextBox.Text = tempString[1];

                SendMessage(tempString);
            }
            else
            {
                xThread_UserList d = new xThread_UserList(usersChanged);
                Invoke(d);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        { UpdateThread.Abort(); }

        private void sendButton_Click(object sender, EventArgs e)
        {
            Command = chatBox.Text;
            chatBox.Text = "";
            Commanded();
        }
    }
}
