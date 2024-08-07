﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using IC.RCS.RCSCore;
using System.Configuration;

namespace IC.RCS.RCSWCF
{
    public class RCSWCFService : IRCSWCFService
    {
        public TrendGroupConfigReaderWriter trendGroupConfigReaderWriter;

        public RCSWCFService (TrendGroupConfigReaderWriter trendGroupConfigReaderWriter)
        {
            this.trendGroupConfigReaderWriter = trendGroupConfigReaderWriter;
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public DataTable GetTrendGroupsConfig()
        {

            return trendGroupConfigReaderWriter.GetTrendGroupsConfigDataTable();

        }
    }
}
