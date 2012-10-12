using System.IO;
using System.Net;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections;

class ChatClient
{
    string Name;
    System.Net.Sockets.TcpClient tcpClient;

    string Message;
    System.Threading.Thread KeyboardThread;

    public ChatClient()
    {
        Console.WriteLine("What is your name?");
        Name = Console.ReadLine();

        Console.WriteLine("Please provide the host's IP address.");
        string ipString = Console.ReadLine();

        byte[] ipAddressBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        { ipAddressBytes[i] = 0; }
        int port = 25565;

        string tempString = "";
        int j = 0;
        for (int i = 0; i < ipString.Length; i++)
        {
            if (ipString[i] != '.' && ipString[i] != ':' && ipString[i] != '\0')
            { tempString += ipString[i]; }
            else
            {
                if (ipString[i] != '\0')
                for (int k = 0; k < tempString.Length; k++)
                { ipAddressBytes[j] += (byte)((tempString[k] - 48) * 
                    System.Math.Pow(10, (tempString.Length - (k + 1)))); }
                j += 1;

                if (ipString[i] == '\0')
                {
                    for (int k = 0; k < tempString.Length; k++)
                    {
                        port += (tempString[k] - 48) * 
                            (int)System.Math.Pow(10, (tempString.Length - (k + 1)));
                    }
                }

                tempString = "";
            }
        }

        System.Net.IPAddress ipAddress = new System.Net.IPAddress(ipAddressBytes);

        Console.WriteLine("Sending information...");
        tcpClient = new System.Net.Sockets.TcpClient();
        tcpClient.Connect(ipAddress, port);
        StreamWriter writer = new StreamWriter(tcpClient.GetStream());
        writer.WriteLine(Name);
        writer.Flush();

        Console.WriteLine("Connected!");

        KeyboardThread = new Thread(new ThreadStart(GetMessage));
        Message = "";

        KeyboardThread.Start();
        Update();
    }

    public void Update()
    {
        bool quit = false;
        while (!quit)
        {
            StreamReader reader = new StreamReader(tcpClient.GetStream());
            Console.WriteLine(reader.ReadLine());
        }
    }

    public void GetMessage()
    {
        while (true)
        {
            Message = Console.ReadLine();

            StreamWriter writer = new StreamWriter(tcpClient.GetStream());
            writer.WriteLine(Message);
            writer.Flush();
        }
    }
}
