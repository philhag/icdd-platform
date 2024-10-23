using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace IcddWebApp.WebApplication.Environment
{
    public class Logger
    {
        public enum MsgType
        {
            Error,
            Info,
            Warning
        }

        public static Logger Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (_SyncRoot)
                    {
                        if (_Instance == null)
                            _Instance = new Logger();
                    }
                }
                return _Instance;
            }
        }

        private static Logger _Instance;
        private static object _SyncRoot = new Object();
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        private Logger()
        {
            LogFileName = DateTime.Now.ToString("dd-MM-yyyy");
            LogFileExtension = ".xml";
            LogPath = "ProgramLog/";
        }

        public StreamWriter Writer { get; set; }

        public string LogPath { get; set; }

        public string LogFileName { get; set; }

        public string LogFileExtension { get; set; }

        public string LogFile { get { return LogFileName + LogFileExtension; } }

        public string LogFullPath { get { return Path.Combine(LogPath, LogFile); } }

        public bool LogExists { get { return File.Exists(LogFullPath); } }

        public void WriteToLog(String inLogMessage, MsgType msgtype, string functionCall)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                LogFileName = DateTime.Now.ToString("dd-MM-yyyy");

                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }

                var settings = new System.Xml.XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true
                };

                StringBuilder sbuilder = new StringBuilder();
                using (StringWriter sw = new StringWriter(sbuilder))
                {
                    using (XmlWriter w = XmlWriter.Create(sw, settings))
                    {
                        w.WriteStartElement("LogEntry");

                        if (msgtype == MsgType.Error)
                            w.WriteAttributeString("Type", "Error");
                        else if (msgtype == MsgType.Info)
                            w.WriteAttributeString("Type", "Info");
                        else if (msgtype == MsgType.Warning)
                            w.WriteAttributeString("Type", "Warning");

                        w.WriteElementString("Time", DateTime.Now.ToString());
                        w.WriteElementString("Message", inLogMessage);
                        w.WriteElementString("Function", functionCall);
                        w.WriteEndElement();
                    }
                }
                using (StreamWriter Writer = new StreamWriter(LogFullPath, true, Encoding.UTF8))
                {
                    Writer.WriteLine(sbuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }

        public static void Log(String inLogMessage, MsgType msgtype, string functionCall)
        {
            Instance.WriteToLog(inLogMessage, msgtype, functionCall);
        }
    }
}
