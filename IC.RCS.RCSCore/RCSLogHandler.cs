using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Configuration;
using System.IO;

namespace IC.RCS.RCSCore
{
    
    public class RCSLogHandler
    {
        private ChannelFactory<IRCSWCFService> _serviceChannelFactory = new ChannelFactory<IRCSWCFService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/RCSTransferService/RCSTransferService"));
        private ChannelFactory<IRCSFormWCFService> _formChannelFactory = new ChannelFactory<IRCSFormWCFService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/RCSTransferForm/RCSTransferForm"));
        private string _sourceName;
        private string _logPath;
        public int fileLineMax = 100000;

        public RCSLogHandler(string sourceName) 
        {
            _sourceName = sourceName;
            GetLogFilePath();
        }

        public void GetLogFilePath()
        {
            IRCSWCFService client = _serviceChannelFactory.CreateChannel();

            try
            {
                string serviceExePath = client.GetExePath();
                Configuration config = ConfigurationManager.OpenExeConfiguration(serviceExePath);
                _logPath = config.AppSettings.Settings["logfilepath"].Value;
            }
            catch 
            {
                _logPath = ConfigurationManager.AppSettings["logfilepath"];
            }
            _logPath = _logPath + "\\LOG_RCSTransferService_" + _sourceName + ".txt";
        }

        public void Log(RCSLogLevel logLevel, dynamic obj)
        {
            string message;

            switch (obj)
            {
                case AggregateException agEx:
                    string line;
                    message = "";
                    foreach (var innerException in agEx.Flatten().InnerExceptions)
                    {
                        line = formatMessage(DateTime.Now, logLevel, innerException.ToString())+Environment.NewLine;
                        message += line;
                    }
                    break;
                case Exception ex:
                    message = formatMessage(DateTime.Now, logLevel, ex.ToString() );
                    break;
                default:
                    message = formatMessage(DateTime.Now, logLevel, obj);
                    break;
            }

            Task.Run(() => writeForm(message));
            Task.Run(() => writeFile(message));
            Task.Run(() => writeConsole(message));

        }

        private string formatMessage(DateTime timeStamp, RCSLogLevel logLevel, string message)
        {
            string stringTemplate = "{0} [{1}] [{2}]: {3}";
            message = String.Format(stringTemplate, timeStamp.ToString("yyyy-MM-dd HH:mm:ss"), _sourceName, logLevel.ToString() ,message);

            return message;
        }

        private void writeForm(string message)
        {
            try
            {
                IRCSFormWCFService client = _formChannelFactory.CreateChannel();
                client.LogForm(message);
            }catch {

            }
            
        }

        private void writeFile(string message)
        {
            try
            {
                GetLogFilePath();
                if (!File.Exists(_logPath))
                {
                    // Create the file
                    using (StreamWriter sw = File.CreateText(_logPath))
                    {
                        sw.WriteLine(message);
                    }
                }
                else
                {
                    // Append to existing file
                    using (StreamWriter sw = File.AppendText(_logPath))
                    {
                        sw.WriteLine(message);
                        tryTrimFile();
                    }
                }
            } catch
            {

            }
            
        }
    
        private void writeConsole(string message)
        {
            Console.WriteLine(message);
        }
    
        private void tryTrimFile()
        {
            GetLogFilePath();

            string[] lines = File.ReadAllLines(_logPath);

            int lineCount = lines.Length;

            if (lineCount > fileLineMax)
            {
                File.WriteAllLines(_logPath, lines.Skip(10000).ToArray() );
            }
        }
    
    }
    
}
