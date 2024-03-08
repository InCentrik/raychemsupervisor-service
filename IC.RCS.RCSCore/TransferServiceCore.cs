﻿using EHT.EHTMain;
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

        private ChannelFactory<IRCSFormWCFService> _formChannelFactory = new ChannelFactory<IRCSFormWCFService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/RCSTransferForm/RCSTransferForm"));

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

        public void LogMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            //Create output to form and to file in configuration
            try
            {
                IRCSFormWCFService channel = _formChannelFactory.CreateChannel();
                channel.LogForm(message);
            } catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
            }
            

        }

        public TransferServiceCore()
        {
            GetTrendGroupConfig();
            GetSqlClientConnectionCredentials();
            sqlClient = new EHTSQLClient(serverName, databaseName, userName, password);
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
                    bool isMonitored = trendGroup.IsMonitored == "true" ? true : false;
                    string guidString = trendGroup.Guid;

                    if (isMonitored)
                    {
                        Console.WriteLine("Starting transfer on " + guidString);
                        Task trendGroupTask = Task.Run(() => TransferTrendGroupData(guidString), cancellationToken);
                        taskList.Add(trendGroupTask);
                    }
                }
                status = ServiceStatus.Started;
            }

        }

        public void StopTrendGroupTransfer()
        {
            if (status == ServiceStatus.Started)
            {
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
                LogMessage("ROUND");

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

                bool readOldData = true;
                foreach (EHTDeviceIDData device in devices)
                {
                    var tData = sqlClient.GetBigTrendDeviceTrendDataByTime(guid, device.DeviceGuid, device.DeviceType, useSmallDataset, maxRecords, startTime, readOldData);
                    string deviceGuidString = device.DeviceGuid.ToString();
                    foreach (tbEHTTrendData trendData in tData)
                    {
                        //TODO: Throw error or skip if trendData.TrendData is null
                        //NOTE:Any new device types will require changing source code below. Automation or user form submission to add more cases may be possible.
                        
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
                config.Save();
                Thread.Sleep(scanRate * 1000);
            }
        }


    }
}
