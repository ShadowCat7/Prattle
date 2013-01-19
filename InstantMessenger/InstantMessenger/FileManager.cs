using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace InstantMessenger
{
    public static class FileManager
    {
        public static bool checkBanList(string address)
        {
            if (File.Exists("ban_list"))
            {
                StreamReader reader = new StreamReader("ban_list");
                while (!reader.EndOfStream)
                {
                    if (reader.ReadLine() == address)
                    { return true; }
                }
                reader.Close();
            }
            return false;
        }

        public static string getBanList()
        {
            if (File.Exists("ban_list"))
            {
                StreamReader reader = new StreamReader("ban_list");
                string tempString = reader.ReadToEnd();
                reader.Close();
                return tempString;
            }
            return "";
        }

        public static void addToBanList(string address)
        {
            if (!File.Exists("ban_list"))
            { File.Create("ban_list").Close(); }

            StreamWriter writer = new StreamWriter("ban_list", true);
            writer.WriteLine(address);
            writer.Flush();
            writer.Close();
        }

        public static bool removeFromBanList(string address)
        {
            if (File.Exists("ban_list"))
            {
                StreamReader reader = new StreamReader("ban_list");
                List<string> bannedIPs = new List<string>();
                while (!reader.EndOfStream)
                { bannedIPs.Add(reader.ReadLine()); }
                reader.Close();
                for (int i = 0; i < bannedIPs.Count; i++)
                {
                    if (address == bannedIPs[i])
                    { bannedIPs.RemoveAt(i); }
                }
                File.Delete("ban_list");
                File.Create("ban_list").Close();
                StreamWriter writer = new StreamWriter("ban_list");
                for (int i = 0; i < bannedIPs.Count; i++)
                { writer.WriteLine(bannedIPs[i]); }
                writer.Close();

                return true;
            }
            return false;
        }
    }
}
