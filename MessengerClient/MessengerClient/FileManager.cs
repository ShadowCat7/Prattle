using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Prattle
{
    public static class FileManager
    {
        public static void getConfiguration()
        {
            if (!File.Exists("config"))
            { newConfigFile(); }

            StreamReader reader = new StreamReader("config");

            if (reader.ReadLine() == "chat history:true")
            { Config.saveChatHistory = true; }
            else
            { Config.saveChatHistory = false; }

            reader.Close();
        }

        public static void changeConfig(string setting, string option)
        {
            if (!File.Exists("config"))
            { newConfigFile(); }

            StreamReader reader = new StreamReader("config");
            List<string> tempString = new List<string>();
            while (!reader.EndOfStream)
            { tempString.Add(reader.ReadLine()); }
            reader.Close();

            for (int i = 0; i < tempString.Count; i++)
            {
                if (tempString[i].Contains(setting))
                { tempString[i] = setting + ":" + option; }
            }

            try
            {
                StreamWriter writer = new StreamWriter("config");
                for (int i = 0; i < tempString.Count; i++)
                { writer.WriteLine(tempString[i]); }
                writer.Close();
            }
            catch (IOException) { }
        }

        private static void newConfigFile()
        {
            File.Create("config").Close();
            StreamWriter writer = new StreamWriter("config");
            writer.AutoFlush = true;

            writer.WriteLine("chat history:true");

            writer.Close();
        }

        public static List<string> getPreviousIps()
        {
            if (!File.Exists("previous_ips"))
            { File.Create("previous_ips").Close(); }
            StreamReader reader = new StreamReader("previous_ips");

            List<string> ipAddresses = new List<string>();
            while (!reader.EndOfStream)
            { ipAddresses.Add(reader.ReadLine()); }
            reader.Close();
            return ipAddresses;
        }

        public static void deleteChatHistory()
        {
            if (File.Exists("chat_history.txt"))
            { File.Delete("chat_history.txt"); }
            File.Create("chat_history.txt").Close();
        }

        public static void writeToChatHistory()
        {
            if (Config.saveChatHistory)
            {
                if (!File.Exists("chat_history.txt"))
                {
                    File.Create("chat_history.txt").Close();
                    StreamWriter writer = new StreamWriter("chat_history.txt");
                    writer.WriteLine("Chat history: On");
                    writer.Flush();
                    writer.Close();
                }
            }
        }
    }
}
