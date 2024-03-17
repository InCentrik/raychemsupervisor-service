using EHT.EHTMain;
using EHT.CommonUI;
using EHT.EHTDataObjectModel;
using EHT.EHTDeviceModel;
using EHT.EHTDefinitions;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows.Forms.PropertyGridInternal;

namespace IC.RCS.RCSCore
{
    public class TransferServiceCore
    {
        public Configuration config;
        public ChannelFactory<IRCSWCFService> _serviceChannelFactory = new ChannelFactory<IRCSWCFService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/RCSTransferService/RCSTransferService"));
        string serverName;
        string databaseName;
        string userName;
        string password;

        private RCSLogHandler _logger = new RCSLogHandler("Core");

        TrendGroupConfig trendGroupConfig;
        EHTSQLClient sqlClient;
        List<Task> taskList = new List<Task>();
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken;
        ServiceStatus status;


        enum ServiceStatus
        {
            Stopped,
            Started
        }

        public TransferServiceCore()
        {
            Thread.Sleep(5000);
            GetTrendGroupConfig();
            GetSqlClientConnectionCredentials();
            sqlClient = new EHTSQLClient(serverName, databaseName, userName, password);
            _logger.Log(RCSLogLevel.Information, "Core transfer service has been initiated");
        }


        private void GetSqlClientConnectionCredentials()
        {
            try
            {
                serverName = ConfigurationManager.AppSettings["servername"];
                databaseName = ConfigurationManager.AppSettings["databasename"];
                userName = ConfigurationManager.AppSettings["username"];
                password = ConfigurationManager.AppSettings["password"];
            }
            catch
            {

            }

        }

        public void StartTrendGroupTransfer()
        {
            if (status == ServiceStatus.Stopped)
            {
                cancellationToken = cancellationTokenSource.Token;
                foreach (TrendGroupElement trendGroup in trendGroupConfig.TrendGroups)
                {
                    string name = trendGroup.Name;
                    bool isMonitored = trendGroup.IsMonitored == "true" ? true : false;
                    string guidString = trendGroup.Guid;

                    if (isMonitored)
                    {
                        _logger.Log(RCSLogLevel.Information, "Starting regular transfer of " + name);

                        Console.WriteLine();
                        Task trendGroupTask = Task.Run(() => TransferTrendGroupData(guidString), cancellationToken);
                        taskList.Add(trendGroupTask);
                    }
                }
                status = ServiceStatus.Started;
            }
            else
            {
            }

        }

        public void StopTrendGroupTransfer()
        {
            if (status == ServiceStatus.Started)
            {
                _logger.Log(RCSLogLevel.Information, "Stop order sent to all trend group transfers");
                cancellationTokenSource.Cancel();
                status = ServiceStatus.Stopped;
            }
        }

        public void GetTrendGroupConfig()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            trendGroupConfig = (TrendGroupConfig)config.GetSection("trendGroupsConfig");
        }

        public void TransferTrendGroupData(string guidString)
        {
            Guid guid = new Guid(guidString);
            GetTrendGroupConfig();

            TrendGroupElement trendGroupElement = trendGroupConfig.TrendGroups[guidString];

            string name = trendGroupElement.Name;
            int scanRate = int.Parse(trendGroupElement.ScanRate);
            int pullDays = int.Parse(trendGroupElement.PullDays);

            string trendGroupTableName = sqlClient.GetFakeTrendGroupTableName(guid);
            while (true)
            {
                try
                {
                    _logger.Log(RCSLogLevel.Information, name + ": transfer initiated");

                    EHTDeviceIDData[] devices = new EHTDeviceIDData[0];
                    sqlClient.GetTrendDevicesIdDataByTrendGroup(guid, out devices);


                    //TODO insert remove table if exists if needed, add faketrend table if needed

                    if (sqlClient.IsDBTableExists(trendGroupTableName))
                        sqlClient.RemoveBigTrendDataTableByName(trendGroupTableName);
                    sqlClient.CreateFakeTrendTable(guid);

                    //NOTE useSmallDataset makes a subset of data to be transferred in GetBigTrendDeviceTrendDataByTime. Doesn't seem necessary at this point.

                    bool useSmallDataset = false;

                    //NOTE maximum number of rows that will be pulled from table
                    int maxRecords = 1000000000;

                    DateTime startTime = DateTime.UtcNow.Subtract(new TimeSpan(pullDays, 0, 0, 0));

                    bool readOldData = false;
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    foreach (EHTDeviceIDData device in devices)
                    {
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        var tData = sqlClient.GetBigTrendDeviceTrendDataByTime(guid, device.DeviceGuid, device.DeviceType, useSmallDataset, maxRecords, startTime, readOldData);
                        string deviceGuidString = device.DeviceGuid.ToString();
                        foreach (tbEHTTrendData trendData in tData)
                        {
                            //NOTE:Any new device types will require changing source code below. Automation or user form submission to add more cases may be possible.
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            if (trendData.TrendData != null)
                            {
                                switch (device.DeviceType)
                                {
                                    case EHTDeviceTypeConstants.HTCDIRECT:
                                    case EHTDeviceTypeConstants.HTC:
                                        if (useSmallDataset)
                                            sqlClient.AddtinyHTCTrendData(deviceGuidString, trendGroupTableName, (tinyEHTHTCValues)trendData.TrendData, trendData.TrendDateTime);
                                        else
                                            sqlClient.AddHTCTrendData(deviceGuidString, trendGroupTableName, (EHTHTCValues)trendData.TrendData, trendData.TrendDateTime);
                                        break;
                                    case EHTDeviceTypeConstants.HTC2:
                                        if (useSmallDataset)
                                            sqlClient.AddtinyHTC2TrendData(deviceGuidString, trendGroupTableName, (tinyEHTHTC2Values)trendData.TrendData, trendData.TrendDateTime);
                                        else
                                            sqlClient.AddHTC2TrendData(deviceGuidString, trendGroupTableName, (EHTHTC2Values)trendData.TrendData, trendData.TrendDateTime);
                                        break;
                                    case EHTDeviceTypeConstants.HTC2P3:
                                        if (useSmallDataset)
                                            sqlClient.AddtinyHTC3TrendData(deviceGuidString, trendGroupTableName, (tinyEHTHTC2Values3P)trendData.TrendData, trendData.TrendDateTime);
                                        else
                                            sqlClient.AddHTC3TrendData(deviceGuidString, trendGroupTableName, (EHTHTC2Values3P)trendData.TrendData, trendData.TrendDateTime);
                                        break;
                                    case EHTDeviceTypeConstants.M3KIO:
                                        if (useSmallDataset)
                                            sqlClient.AddtinyIOTrendData(deviceGuidString, trendGroupTableName, (tinyM3KIOValues)trendData.TrendData, trendData.TrendDateTime);
                                        else
                                            sqlClient.AddIOTrendData(deviceGuidString, trendGroupTableName, (M3KIOValues)trendData.TrendData, trendData.TrendDateTime);
                                        break;
                                    case EHTDeviceTypeConstants.M3KLIMITER:
                                        if (useSmallDataset)
                                            sqlClient.AddtinySLIMTrendData(deviceGuidString, trendGroupTableName, (tinyM3KLimiterValues)trendData.TrendData, trendData.TrendDateTime);
                                        else
                                            sqlClient.AddSLIMTrendData(deviceGuidString, trendGroupTableName, (M3KLimiterValues)trendData.TrendData, trendData.TrendDateTime);
                                        break;
                                    case EHTDeviceTypeConstants.LOOP2:
                                        if (useSmallDataset)
                                            sqlClient.AddtinyNGC30Circuit2TrendData(deviceGuidString, trendGroupTableName, (tinyEHTCircuit2Values)trendData.TrendData, trendData.TrendDateTime);
                                        else
                                            sqlClient.AddNGC30Circuit2TrendData(deviceGuidString, trendGroupTableName, (EHTCircuit2Values)trendData.TrendData, trendData.TrendDateTime);
                                        break;
                                }
                            }
                        }
                    }

                    trendGroupElement.LastRefreshTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    config.Save(ConfigurationSaveMode.Modified);

                    GetTrendGroupConfig();
                    trendGroupElement = trendGroupConfig.TrendGroups[guidString];


                    _logger.Log(RCSLogLevel.Information, name + ": transfer completed");
                }
                catch (OperationCanceledException) { 
                    _logger.Log(RCSLogLevel.Information, name + ": transfer stopped");
                }
                catch (Exception exc)
                {
                    _logger.Log(RCSLogLevel.Error, exc);
                } finally
                {
                    Thread.Sleep(scanRate * 1000);
                }


            }
        }


    }
}
