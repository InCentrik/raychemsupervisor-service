using EHT.EHTDefinitions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IC.RCS.RCSCore
{
    public class TrendGroupConfigReaderWriter
    {
        private TrendGroupConfig _trendGroupsConfig;

        public TrendGroupConfigReaderWriter(Configuration config)
        {
            _trendGroupsConfig = (config is null) ? (TrendGroupConfig)ConfigurationManager.GetSection("trendGroupsConfig") : (TrendGroupConfig)config.GetSection("trendGroupsConfig") ;
        }

        public DataTable GetTrendGroupsConfigDataTable()
        {
            DataTable dt = new DataTable();
            
            foreach (TrendGroupElement trendGroup in _trendGroupsConfig.TrendGroups)
            {
                DataRow dr = dt.NewRow();
                dr["GUID"] = trendGroup.Guid;
                dr["Name"] = trendGroup.Name;
                dr["Description"] = trendGroup.Description;
                dr["IsMonitored"] = trendGroup.IsMonitored;
                dr["LastRefreshTime"] = trendGroup.LastRefreshTime;
                dr["ScanRate"] = trendGroup.ScanRate;

                dt.Rows.Add(dr);

            }

            return dt;
        }
    }
}
