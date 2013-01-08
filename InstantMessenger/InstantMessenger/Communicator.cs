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
        public string ipAddress;

        public string Message;

        public Thread ReceiveMessage;

        public Communicator(System.Net.Sockets.TcpClient connection)
        {
            client = connection;
            writer = new StreamWriter(client.GetStream());
            reader = new StreamReader(client.GetStream());

            ipAddress = reader.ReadLine();
            Name = reader.ReadLine();

            ReceiveMessage = new Thread(new ThreadStart(getMessage));
            ReceiveMessage.IsBackground = true;
            ReceiveMessage.Start();
        }

        public void getMessage()
        {
            while (true)
            {
                try
                {
                    Message = reader.ReadLine();
                    if (reader.EndOfStream)
                    {
                        client.Close();
                        ReceiveMessage.Abort();
                    }
                }
                catch (IOException)
                { client.Close(); }
                catch (ObjectDisposedException)
                { reader.Dispose(); }
            }
        }
    }
}