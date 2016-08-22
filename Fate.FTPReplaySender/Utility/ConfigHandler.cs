using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fate.FTPReplaySender.Utility
{
    public class ConfigHandler
    {
        public string ReplayPath { get; private set; }
        public string FTPAddr { get; set; }
        public string FTPUserName { get; set; }
        public string FTPPassword { get; set; }
        public int ParseTimePeriod { get; set; }

        private string _configFilePath;
        private readonly List<string> _fileContent = new List<string>();
        public ConfigHandler(string configFilePath)
        {
            _configFilePath = configFilePath;
            if (File.Exists(configFilePath))
            {
                _fileContent = new List<string>(File.ReadAllLines(configFilePath));
            }
        }

        public bool IsConfigFileValid()
        {
            return _fileContent.Any();
        }

        public void LoadConfig()
        {
            ReplayPath = GetConfigString("replaypath");
            FTPAddr = GetConfigString("ftpaddr");
            FTPUserName = GetConfigString("ftpusername");
            FTPPassword = GetConfigString("ftppassword");
            int parsedInt;
            if (int.TryParse(GetConfigString("parsetimeperiod"), out parsedInt))
            {
                ParseTimePeriod = parsedInt;
            }
        }

        private string GetConfigString(string key)
        {
            foreach (string line in _fileContent)
            {
                if (String.IsNullOrEmpty(line))
                    continue;
                if (line[0] == '#') //Config Comment
                    continue;
                string[] configValueArray = line.Split('=');
                if (configValueArray.Length != 2)
                    continue;
                if (configValueArray[0] == key)
                    return configValueArray[1];
            }
            return String.Empty;
        }
    }
}
