using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using Fate.FTPReplaySender.Utility;
using Limilabs.FTP.Client;
using NLog;

namespace Fate.FTPReplaySender
{
    internal static class Program
    {
        private const int CONSOLE_CLEAR_DISPLAY_LIMIT = 50; //Clears console every nth time parsing has run
        private const string DEFAULT_CONFIG_FILE_PATH = "config.cfg";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static string _replayFileDirectory;
        private static string _sentReplayDirectory = "SentReplay";
        private static Timer _parseTimer;
        private static int _parsePeriod;
        private static int _consoleClearCounter = 0;
        private static string _ftpAddr;
        private static string _ftpUserName;
        private static string _ftpPassword;

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

            _replayFileDirectory = configHandler.ReplayPath;
            if (!String.IsNullOrEmpty(_replayFileDirectory)) {
                logger.Trace("Using Warcraft 3 Replay File Directory from config file: {0}", _replayFileDirectory);
            }
            else
            {
                logger.Trace("Replay directory not set. Terminating.");
                return;
            }

            if (String.IsNullOrEmpty(configHandler.FTPAddr) || String.IsNullOrEmpty(configHandler.FTPPassword) ||
                String.IsNullOrEmpty(configHandler.FTPUserName))
            {
                logger.Trace("FTP information not properly set in config file. Terminating.");
                return;
            }

            _ftpAddr = configHandler.FTPAddr;
            _ftpUserName = configHandler.FTPUserName;
            _ftpPassword = configHandler.FTPPassword;

            if (configHandler.ParseTimePeriod < 5)
            {
                logger.Trace("Setting minimum default parsing period of 5 seconds.");
                _parsePeriod = 5;
            }
            else
            {
                logger.Trace($"Setting parsing period of {configHandler.ParseTimePeriod} seconds from config file.");
                _parsePeriod = configHandler.ParseTimePeriod;
            }

            _parseTimer = new Timer(_parsePeriod * 1000);
            _parseTimer.Elapsed += (sender, e) => RunParser(sender, e, configHandler);
            RunParser(null, null, configHandler);

            while (true)
            {
                Console.ReadLine();
            }
        }

        private static void MoveFile(DirectoryInfo directory, FileInfo file)
        {
            string pathToMoveTo = Path.Combine(directory.FullName, file.Name);
            if (!directory.Exists)
            {
                logger.Trace($"Creating directory {directory.FullName}");
                Directory.CreateDirectory(directory.FullName);
            }
            if (File.Exists(pathToMoveTo))
            {
                logger.Trace($"Duplicate file name found for {file.Name} at {directory.FullName} directory. File deleted.");
                file.Delete();
            }
            else
            {
                file.MoveTo(Path.Combine(directory.FullName, file.Name));
            }
        }

        private static void RunParser(object sender, ElapsedEventArgs e, ConfigHandler configHandler)
        {
            _parseTimer.Stop();
            
            DirectoryInfo replayDirectory = new DirectoryInfo(_replayFileDirectory);
            DirectoryInfo sentDirectory = new DirectoryInfo(_sentReplayDirectory);
            if (!replayDirectory.GetFiles().Any())
            {
                logger.Trace($"No replay files found in directory.");
            }
            else
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                logger.Trace("-----------------------------------------");

                using (Ftp client = new Ftp())
                {
                    client.Connect(_ftpAddr);    
                    client.Login(_ftpUserName, _ftpPassword);

                    foreach (var file in replayDirectory.GetFiles())
                    {
                        try
                        {
                            logger.Trace("Sending replay file to server: " + file.Name);

                            client.Upload(file.Name, file.FullName);

                            logger.Trace("Finished sending replay file: " + file.Name);
                            MoveFile(sentDirectory, file);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error occurred on sending the following replay file: " + file.Name + Environment.NewLine);
                            logger.Trace(ex + Environment.NewLine);
                        }
                    }
                    client.Close();
                }

                sw.Stop();
                logger.Trace($"Replay send complete [Elapsed: {sw.Elapsed.TotalSeconds} seconds]");
            }

            _consoleClearCounter++;
            if (_consoleClearCounter >= CONSOLE_CLEAR_DISPLAY_LIMIT)
            {
                _consoleClearCounter = 0;
                Console.Clear();
            }
            _parseTimer.Start();
        }
    }
}
