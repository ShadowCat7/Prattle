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
    public static class Server
    {
        private static System.Net.Sockets.TcpListener chatServer;
        private static System.Net.IPAddress localIP;
        private static System.Net.IPAddress extIP;
        private static string domainAddress;

        public static List<Communicator> users;

        public delegate void WriteMessageFunc(string message);
        private static WriteMessageFunc textBoxWriter;

        public delegate void ChangeUserFunc();
        public static ChangeUserFunc userChanger;

        private static Thread updateThread;

        public static void initialize(WriteMessageFunc argWriteMessage, ChangeUserFunc argUserChanger)
        {
            localIP = getLocalIP();
            extIP = getExtIP();
            domainAddress = getDomainAddress();

            chatServer = new System.Net.Sockets.TcpListener(localIP, 24658);
            chatServer.Start();

            textBoxWriter = argWriteMessage;
            userChanger = argUserChanger;

            users = new List<Communicator>();

            updateThread = new Thread(new ThreadStart(updateServer));
            updateThread.IsBackground = true;
            updateThread.Start();
        }

        public static void updateServer()
        {
            while (true)
            {
                System.Net.Sockets.TcpClient chatConnection =
                    chatServer.AcceptTcpClient();

                Communicator newConnection = new Communicator(chatConnection, sendMessage);

                if (FileManager.checkBanList(newConnection.ipAddress))
                {
                    newConnection.writer.WriteLine("/%text%/");
                    newConnection.writer.WriteLine("You have been banned from this server.%/");
                    newConnection.writer.Flush();
                    newConnection.client.Close();
                }
                else
                {
                    if (newConnection.name != "Unnamed user")
                    {
                        List<string> tempString = new List<string>();
                        tempString.Add("/%text%/");
                        tempString.Add("\t" + newConnection.name +
                            " has joined the chat.%/");

                        users.Add(newConnection);
                        userChanger();

                        sendMessage(tempString);
                    }
                }
            }
        }

        public static void sendMessage(List<string> message)
        {
            for (int i = 0; i < users.Count; i++)
            { users[i].sendMessage(message); }

            if (message.Count == 2)
            {
                if (message[0] == "/%text%/")
                { textBoxWriter(message[1]); }
            }
        }

        public static void kickUser(string name)
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
        public static string banUser(string name)
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

        public static int getUserCount()
        { return users.Count; }

        public static List<string> getUserNames()
        {
            List<string> userNames = new List<string>();
            for (int i = 0; i < users.Count; i++)
            { userNames.Add(users[i].name); }
            return userNames;
        }

        public static IPAddress getLocalIP()
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

        public static IPAddress getExtIP()
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

        public static string getDomainAddress()
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
