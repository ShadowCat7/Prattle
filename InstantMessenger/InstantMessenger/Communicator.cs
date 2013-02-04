using System.IO;
using System.Net;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections;

namespace InstantMessenger
{
    public class Communicator
    {
        public System.Net.Sockets.TcpClient client;
        public StreamWriter writer;
        public StreamReader reader;
        public string name;
        public string ipAddress;

        public delegate void WriteMessageFunc(List<string> message);
        public WriteMessageFunc textBoxWriter;

        public Thread receiveMessage;

        public Communicator(System.Net.Sockets.TcpClient connection, WriteMessageFunc messageGotten)
        {
            client = connection;
            writer = new StreamWriter(client.GetStream());
            reader = new StreamReader(client.GetStream());

            try
            {
                //This will block other people from joining. Fix.
                ipAddress = reader.ReadLine();
                name = reader.ReadLine();
                if (name == null)
                { name = "Unnamed user"; }
            }
            catch
            { name = "Unnamed user"; }
            textBoxWriter = messageGotten;

            receiveMessage = new Thread(new ThreadStart(getMessage));
            receiveMessage.IsBackground = true;
            receiveMessage.Start();
        }

        public void getMessage()
        {
            while (true)
            {
                try
                {
                    string message = reader.ReadLine();
                    if (reader.EndOfStream)
                    { userDisconnect(); }
                    if (message != null)
                    {
                        List<string> tempList = new List<string>();
                        tempList.Add("/%text%/");
                        tempList.Add(name + ": " + message + "%/");
                        textBoxWriter(tempList);
                    }
                }
                catch (IOException)
                { userDisconnect(); }
            }
        }

        private void userDisconnect()
        {
            client.Close();
            Server.users.Remove(this);
            Server.userChanger();

            List<string> tempList = new List<string>();
            tempList.Add("/%text%/");
            tempList.Add("\t" + name + " has disconnected." + "%/");
            textBoxWriter(tempList);

            receiveMessage.Abort();
        }

        public void sendMessage(List<string> message)
        {
            try
            {
                writer.WriteLine(message[0]);
                writer.WriteLine(message[1]);
                writer.Flush();
            }
            catch { }
        }
    }
}