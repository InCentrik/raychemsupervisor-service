using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using IC.RCS.RCSCore;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace IC.RCS.RCSCore
{
    public class RCSWCFService : IRCSWCFService
    {
        public TrendGroupConfigReaderWriter trendGroupConfigReaderWriter;
        public TransferServiceCore transferServiceCore;

        public RCSWCFService()
        {
            transferServiceCore = new TransferServiceCore();
            StartTrendGroupTransfer();
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public string GetExePath()
        {
            return System.Reflection.Assembly.GetEntryAssembly().Location;
        }

        public bool ConfigureSQLConnection(string serverName, string databaseName, string username, string password)
        {
            bool isConnected = false;

            EHTSQLClient sqlClient = new EHTSQLClient(serverName, databaseName, username, password);
            isConnected = sqlClient.TestConnection();

            return isConnected;
        }

        public void PullTrendGroupsFromSQL(string serverName, string databaseName, string username, string password)
        {
            EHTSQLClient sqlClient = new EHTSQLClient(serverName, databaseName, username, password);
            DataSet ds = sqlClient.GetTrendGroups();

            string guid;
            string name;
            string description;
            string ismonitored;
            string lastrefreshtime;
            string scanrate;

            Configuration config = ConfigurationManager.OpenExeConfiguration(0);
            TrendGroupConfig trendGroupsConfig = (TrendGroupConfig)config.GetSection("trendGroupsConfig");

            foreach (DataRow dr in ds.Tables["EHTTrendGroup"].Rows)
            {
                guid = dr["TrendGroupGuid"].ToString();
                name = dr["TrendGroupName"].ToString();

                description = "";
                ismonitored = "false";
                lastrefreshtime = "1970/01/01 00:00:00";
                scanrate = "60";


                bool guidExists = false;
                foreach (TrendGroupElement trendGroup in trendGroupsConfig.TrendGroups)
                {
                    if (trendGroup.Guid == guid)
                    {
                        guidExists = true;
                    }
                }

                if (!guidExists)
                {
                    TrendGroupElement trendGroup = new TrendGroupElement();
                    trendGroup.Guid = guid;
                    trendGroup.Name = name;
                    trendGroup.Description = description;
                    trendGroup.IsMonitored = ismonitored;
                    trendGroup.LastRefreshTime = lastrefreshtime;
                    trendGroup.ScanRate = scanrate;

                    trendGroupsConfig.TrendGroups.Add(trendGroup);
                    config.Save();
                }
                
            }

            

        }

        public void StartTrendGroupTransfer()
        {
            transferServiceCore.StartTrendGroupTransfer();
        }

        public void StopTrendGroupTransfer()
        {
            transferServiceCore.StopTrendGroupTransfer();
        }
    }
}
