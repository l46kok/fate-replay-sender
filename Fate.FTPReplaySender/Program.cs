using System;
using Fate.FTPReplaySender.Utility;
using NLog;

namespace Fate.FTPReplaySender
{
    internal static class Program
    {
        private const int CONSOLE_CLEAR_DISPLAY_LIMIT = 50; //Clears console every nth time parsing has run
        private const string DEFAULT_CONFIG_FILE_PATH = "config.cfg";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            logger.Trace("Starting FTP Replay Sender");
            ConfigHandler configHandler = new ConfigHandler(DEFAULT_CONFIG_FILE_PATH);
            if (!configHandler.IsConfigFileValid())
                logger.Trace("Error loading default config file: {0}", DEFAULT_CONFIG_FILE_PATH);
            else
            {
                logger.Trace("Loading default config file: {0}", DEFAULT_CONFIG_FILE_PATH);
                configHandler.LoadConfig();
            }
        }
    }
}
