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

        private List<Communicator> users;

        private Thread updateThread;

        public delegate void WriteMessageFunc(string message);
        public WriteMessageFunc textBoxWriter;
        public delegate void ChangeUserFunc();
        public ChangeUserFunc userChanger;

        public Server(WriteMessageFunc messageWriterFunc, ChangeUserFunc userChangerFunc)
        {
            localIP = getLocalIP();
            extIP = getExtIP();
            domainAddress = getDomainAddress();

            chatServer = new System.Net.Sockets.TcpListener(localIP, 24658);
            chatServer.Start();

            users = new List<Communicator>();

            textBoxWriter = messageWriterFunc;
            userChanger = userChangerFunc;

            updateThread = new Thread(new ThreadStart(updateServer));
            updateThread.IsBackground = true;
            updateThread.Start();
        }

        public void updateServer()
        {
            while (true)
            {
                if (chatServer.Pending())
                {
                    System.Net.Sockets.TcpClient chatConnection =
                        chatServer.AcceptTcpClient();

                    users.Add(new Communicator(chatConnection, sendMessage));
                    if (FileManager.checkBanList(users[users.Count - 1].ipAddress))
                    {
                        users[users.Count - 1].writer.WriteLine("/%text%/");
                        users[users.Count - 1].writer.WriteLine("You have been banned from this server.%/");
                        users[users.Count - 1].writer.Flush();
                        users[users.Count - 1].client.Close();
                        users.RemoveAt(users.Count - 1);
                    }
                    else
                    {
                        userChanger();

                        List<string> tempString = new List<string>();
                        tempString.Add("/%text%/");
                        tempString.Add("\t" + users[users.Count - 1].name +
                            " has joined the chat.%/");

                        sendMessage(tempString);
                    }
                }

                for (int i = 0; i < users.Count; i++)
                {
                    if (!users[i].client.Connected)
                    {
                        List<string> tempString = new List<string>();
                        tempString.Add("/%text%/");
                        if (users[i].name == null)
                        { users[i].name = "unknown_user"; }
                        tempString.Add(users[i].name + " has disconnected.%/");
                        sendMessage(tempString);
                        try
                        { users.Remove(users[i]); }
                        catch (ArgumentOutOfRangeException) { }
                        userChanger();
                    }
                }
            }
        }

        public void sendMessage(List<string> message)
        {
            for (int i = 0; i < users.Count; i++)
            { users[i].sendMessage(message); }

            if (message[0] == "/%text%/")
            { textBoxWriter(message[1]); }
        }

        public void kickUser(string name)
        {
            int index = 0;
            for (int i = 0; i < users.Count; i++)
            {
                if (name == users[i].name)
                { index = i; }
            }

            List<string> kickMessage = new List<string>();
            kickMessage.Add("/%text%/");
            kickMessage.Add("You have been kicked from the server.%/");
            users[index].sendMessage(kickMessage);
            users[index].receiveMessage.Abort();
            users[index].client.Close();
            users.RemoveAt(index);
        }
        public string banUser(string name)
        {
            int index = 0;
            for (int i = 0; i < users.Count; i++)
            {
                if (name == users[i].name)
                { index = i; }
            }

            List<string> banMessage = new List<string>();
            banMessage.Add("/%text%/");
            banMessage.Add("You have been banned from the server.%/");
            users[index].sendMessage(banMessage);
            string storeIpAddress = users[index].ipAddress;
            users[index].receiveMessage.Abort();
            users[index].client.Close();
            users.RemoveAt(index);
            return storeIpAddress;
        }

        public int getUserCount()
        { return users.Count; }

        public List<string> getUserNames()
        {
            List<string> userNames = new List<string>();
            for (int i = 0; i < users.Count; i++)
            { userNames.Add(users[i].name); }
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
