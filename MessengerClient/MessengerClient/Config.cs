using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prattle
{
    public static class Config
    {
        private static bool _saveChatHistory;
        public static bool saveChatHistory
        {
            get { return _saveChatHistory; }
            set
            {
                FileManager.changeConfig("chat history", value.ToString().ToLower());
                _saveChatHistory = value;
            }
        }
    }
}
