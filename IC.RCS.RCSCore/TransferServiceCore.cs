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

namespace IC.RCS.RCSCore
{
    public class TransferServiceCore
    {
        public Configuration config;
        TrendGroupConfig trendGroupConfig;
        EHTSQLClient sqlClient;

        public TransferServiceCore(string serverName, string databaseName, string password, string username)
        {
            //TODO Make sqlClient params come from config file
            GetTrendGroupConfig();
            sqlClient = new EHTSQLClient(serverName, databaseName, password, username);
        }

        public void GetTrendGroupConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(0);
            TrendGroupConfig trendGroupConfig = (TrendGroupConfig)config.GetSection("trendGroupsConfig");
        }

        public void TransferTrendGroupData(string guidString)
        {
            Guid guid = new Guid(guidString);
            GetTrendGroupConfig();

            TrendGroupElement trendGroupElement = trendGroupConfig.TrendGroups[guidString];

            string name = trendGroupElement.Name;
            int scanRate = int.Parse(trendGroupElement.ScanRate);

            string trendGroupTableName = sqlClient.GetFakeTrendGroupTableName(guid);
            while (true)
            {
                EHTDeviceIDData[] devices = new EHTDeviceIDData[0];
                sqlClient.GetTrendDevicesIdDataByTrendGroup(guid, out devices);


                //TODO insert remove table if exists if needed, add faketrend table if needed

                if (sqlClient.IsDBTableExists(trendGroupTableName))
                    sqlClient.RemoveBigTrendDataTableByName(trendGroupTableName);
                sqlClient.CreateFakeTrendTable(guid);

                //TODO ADD TRENDLOGOPTIONS TO READ IF SMALL DATASET
                bool useSmallDataset = false;

                //add maxrecord
                int maxRecords = 10000;

                bool readOldData = false;

                foreach (EHTDeviceIDData device in devices)
                {
                    var tData = sqlClient.GetBigTrendDeviceTrendDataByTime(guid, device.DeviceGuid, device.DeviceType, useSmallDataset, maxRecords, DateTime.UtcNow, readOldData);
                    string deviceGuidString = device.DeviceGuid.ToString();
                    foreach (tbEHTTrendData trendData in tData)
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

                trendGroupElement.LastRefreshTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                config.Save();
                Thread.Sleep(scanRate);
            }
        }
    }
}
