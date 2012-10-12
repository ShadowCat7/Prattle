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
        public string Name;

        public string Message;

        private Thread ReceiveMessage;

        public Communicator(System.Net.Sockets.TcpClient connection)
        {
            client = connection;
            writer = new StreamWriter(client.GetStream());
            reader = new StreamReader(client.GetStream());

            Name = reader.ReadLine();

            ReceiveMessage = new Thread(new ThreadStart(getMessage));
            ReceiveMessage.Start();
        }

        public void getMessage()
        {
            while (true)
            {
                try
                { Message = reader.ReadLine(); }
                catch (Exception IOException)
                { client.Close(); }
            }
        }
    }
}