using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using IIB.ICDD.Logging;

namespace IcddWebApp.PageModels.Admin
{
    public class LoggingPageModel
    {
        public List<Log> LogFiles = new List<Log>();

        public LoggingPageModel(List<Log> logs)
        {            
               LogFiles = logs;
        }
    }



    public class Log
    {
        public DateTime date;
        private readonly string file;
        private readonly List<LogEntry> logEntries;

        public Log(string file)
        {
            this.file = file;
            var format = "dd-MM-yyyy";
            var provider = new CultureInfo("de-DE");
            var dt = file.Split("/").Last().Split(".").First();
            date = DateTime.ParseExact(dt, format, provider);
            logEntries = new List<LogEntry>();
            try
            {
                var settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                var fStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                using (var reader = XmlReader.Create(fStream, settings))
                {
                    while (reader.ReadToFollowing("LogEntry"))
                    {
                        var type = reader.GetAttribute("Type");
                        reader.ReadToFollowing("Time");
                        var time = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("Message");
                        var message = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("Function");
                        var function = reader.ReadElementContentAsString();
                        var format2 = "dd.MM.yyyy HH:mm:ss";
                        var timeConverted = DateTime.ParseExact(time, format2, provider);
                        LogEntryType entryType;
                        switch (type)
                        {
                            case "Info":
                                entryType = LogEntryType.Info;
                                break;
                            case "Warning":
                                entryType = LogEntryType.Warning;
                                break;
                            case "Error":
                                entryType = LogEntryType.Error;
                                break;
                            default:
                                entryType = LogEntryType.Error;
                                break;
                        }
                        logEntries.Add(new LogEntry(message, entryType, timeConverted, function));
                    }
                    reader.Close();
                }
                fStream.Close();
            }
            catch (Exception e)
            {
                Logger.Log("Could not parse Log File. Exception: " + e, Logger.MsgType.Error,
                    "IcddToolkit.Pages.admin.Log(...");
            }
            logEntries.Reverse();
        }

        public override string ToString()
        {
            return date.ToShortDateString() + ".xml";
        }

        public List<LogEntry> GetEntries()
        {
            return logEntries;
        }

        public string GetFilePath()
        {
            return file;
        }
    }

    public class LogEntry
    {
        private DateTime dtDateTime;
        private readonly string function;
        private readonly string sValue;
        private readonly LogEntryType type;

        public LogEntry(string val, LogEntryType t, DateTime d, string func)
        {
            sValue = val;
            type = t;
            dtDateTime = d;
            function = func;
        }

        public string GetTime()
        {
            return dtDateTime.ToShortTimeString();
        }

        public DateTime GetTimeElement()
        {
            return dtDateTime;
        }

        public string GetValue()
        {
            return sValue;
        }

        public LogEntryType GetEntryType()
        {
            return type;
        }

        public string GetFunction()
        {
            return function;
        }
    }

    public enum LogEntryType
    {
        Info,
        Error,
        Warning
    }
}
