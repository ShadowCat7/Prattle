using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Prattle
{
    public class Client
    {
        private TcpClient tcpClient;
        private StreamWriter writer;

        private Thread serverReadThread;

        public delegate void TextBoxWriter(string message);
        public TextBoxWriter messageWriter;
        public TextBoxWriter userChanger;

        public Client(TextBoxWriter messageWriterFunc, TextBoxWriter userChangerFunc)
        {
            tcpClient = new TcpClient();
            messageWriter = messageWriterFunc;
            userChanger = userChangerFunc;
        }
        public Client(TcpClient client, TextBoxWriter messageWriterFunc, TextBoxWriter userChangerFunc)
        {
            messageWriter = messageWriterFunc;
            userChanger = userChangerFunc;

            instantiate(client);
        }
        private void instantiate(TcpClient client)
        {
            tcpClient = client;
            writer = new StreamWriter(tcpClient.GetStream());
            writer.WriteLine(getExtIP());
            writer.Flush();

            serverReadThread = new Thread(new System.Threading.ThreadStart(readFromServer));
            serverReadThread.IsBackground = true;
            serverReadThread.Start();
        }

        private void readFromServer()
        {
            while (true)
            {
                try
                {
                    StreamReader reader;
                    if (tcpClient.Connected)
                    {
                        reader = new StreamReader(tcpClient.GetStream());
                        try
                        {
                            string output = reader.ReadLine();
                            if (reader.EndOfStream)
                            { disconnect(); }
                            else
                            {
                                if (output != "")
                                {
                                    if (output == "/%text%/")
                                    {
                                        output = reader.ReadLine();
                                        messageWriter(output);
                                    }
                                    if (output == "/%users%/")
                                    {
                                        output = reader.ReadLine();
                                        userChanger(output);
                                    }
                                }
                            }
                        }
                        catch (IOException)
                        { disconnect(); }
                        catch (InvalidOperationException)
                        { disconnect(); }
                    }
                }
                catch (NullReferenceException)
                { }
            }
        }

        public void connect(string stream)
        {
            System.Net.IPAddress tempIP;
            if (System.Net.IPAddress.TryParse(stream, out tempIP))
            {
                try
                { tcpClient.Connect(tempIP, 24658); }
                catch { }
            }
            else
            {
                try
                { tcpClient.Connect(stream, 24658); }
                catch { }
            }

            if (tcpClient.Connected)
            {
                instantiate(tcpClient);
                messageWriter("You have been connected.\n");
                messageWriter("Please type your name.\n");
            }
            else
            { messageWriter("There is no server running at that address.\n"); }
        }

        public bool sendMessage(string message)
        {
            try
            {
                if (tcpClient.Connected)
                {
                    writer.WriteLine(message);
                    writer.Flush();
                    return true;
                }
                else
                { return false; }
            }
            catch (NullReferenceException)
            { return false; }
        }

        public void disconnect()
        {
            if (tcpClient.Connected)
            {
                tcpClient.Close();
                messageWriter("You are no longer connected to a server.\n");
            }
            else
            { messageWriter("You are not connected to a server.\n"); }
            tcpClient = new TcpClient();
            userChanger("Users:%/None");
            serverReadThread.Abort();
        }

        private string getExtIP()
        {
            while (true)
            {
                try
                {
                    System.Net.WebRequest request = System.Net.WebRequest.Create("http://www.jsonip.com/");
                    System.Net.WebResponse response = request.GetResponse(); //TODO
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
                    return ipString;
                }
                catch (WebException) { }
            }
        }
    }
}
