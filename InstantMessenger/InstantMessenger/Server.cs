using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace InstantMessenger
{
    class Server
    {
        private System.Net.Sockets.TcpListener chatServer;
        private System.Net.IPAddress localIP;
        private System.Net.IPAddress extIP;
        private string domainAddress;

        private List<Communicator> Users;

        private Thread UpdateThread;

        private delegate void WriteMessageFunc(string message);
        private WriteMessageFunc textBoxWriter;
        private delegate void ChangeUserFunc();
        private ChangeUserFunc userChanger;

        public Server(WriteMessageFunc messageWriterFunc, ChangeUserFunc userChangerFunc)
        {
            localIP = getLocalIP();
            extIP = getExtIP();
            domainAddress = getDomainAddress();

            chatServer = new System.Net.Sockets.TcpListener(localIP, 24658);
            chatServer.Start();

            Users = new List<Communicator>();

            textBoxWriter = messageWriterFunc;
            userChanger = userChangerFunc;

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
                        userChanger();

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
                        userChanger();
                    }
                }
            }
        }

        public void SendMessage(List<string> message)
        {
            for (int i = 0; i < Users.Count; i++)
            { Users[i].sendMessage(message); }

            if (message[0] == "/%text%/")
            {
                string tempString = "";
                if (message.Count > 0)
                { tempString = convertToReturns(message[1]); }
                textBoxWriter(tempString);
            }
        }

        public void kickUser(string name)
        {
            int index = 0;
            for (int i = 0; i < Users.Count; i++)
            {
                if (name == Users[i].Name)
                { index = i; }
            }

            List<string> kickMessage = new List<string>();
            kickMessage.Add("/%text%/");
            kickMessage.Add("You have been kicked from the server.");
            Users[index].sendMessage(kickMessage);
            Users[index].ReceiveMessage.Abort();
            Users[index].client.Close();
            Users.RemoveAt(index);
        }
        public string banUser(string name)
        {
            int index = 0;
            for (int i = 0; i < Users.Count; i++)
            {
                if (name == Users[i].Name)
                { index = i; }
            }

            List<string> banMessage = new List<string>();
            banMessage.Add("/%text%/");
            banMessage.Add("You have been banned from the server.");
            Users[index].sendMessage(banMessage);
            string storeIpAddress = Users[index].ipAddress;
            Users[index].ReceiveMessage.Abort();
            Users[index].client.Close();
            Users.RemoveAt(index);
            return storeIpAddress;
        }

        public int getUserCount()
        { return Users.Count; }

        public List<string> getUserNames()
        {
            List<string> userNames = new List<string>();
            for (int i = 0; i < Users.Count; i++)
            { userNames.Add(userNames[i]); }
            return userNames;
        }

        public IPAddress getLocalIP()
        {
            if (localIP == null)
            {
                IPHostEntry host;
                string tempIP = "";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.ToString() == "InterNetwork")
                    { tempIP = ip.ToString(); }
                }
                return System.Net.IPAddress.Parse(tempIP);
            }
            else
            { return localIP; }
        }

        public IPAddress getExtIP()
        {
            if (extIP == null)
            {
                while (true)
                {
                    try
                    {
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
                        reader.Close();
                        return IPAddress.Parse(ipString);
                    }
                    catch (WebException) { }
                }
            }
            else
            { return extIP; }
        }

        public string getDomainAddress()
        {
            if (domainAddress == null)
            {
                IPHostEntry entry = Dns.GetHostEntry(extIP);
                return entry.HostName;
            }
            else
            { return domainAddress; }
        }
    }
}
