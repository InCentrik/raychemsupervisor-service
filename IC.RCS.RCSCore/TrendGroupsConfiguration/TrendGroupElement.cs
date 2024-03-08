using System.Configuration;

namespace IC.RCS.RCSCore
{
    public class TrendGroupElement : ConfigurationElement
    {
        [ConfigurationProperty("guid", IsKey = true, IsRequired = true)]
        public string Guid
        {
            get
            {
                return (string)base["guid"];
            }
            set
            {
                base["guid"] = value;
            }
        }

        [ConfigurationProperty("name",IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        [ConfigurationProperty("description", IsRequired = true)]
        public string Description
        {
            get
            {
                return (string)base["description"];
            }
            set
            {
                base["description"] = value;
            }
        }

        [ConfigurationProperty("ismonitored", IsRequired = true)]
        public string IsMonitored
        {
            get
            {
                return (string)base["ismonitored"];
            }
            set
            {
                base["ismonitored"] = value.ToString();
            }
        }

        [ConfigurationProperty("lastrefreshtime", IsRequired = true)]
        public string LastRefreshTime
        {
            get
            {
                return (string)base["lastrefreshtime"];
            }
            set
            {
                base["lastrefreshtime"] = value.ToString();
            }
        }

        [ConfigurationProperty("scanrate", IsRequired = true)]
        public string ScanRate
        {
            get
            {
                return (string)base["scanrate"];
            }
            set
            {
                base["scanrate"] = value.ToString();
            }
        }

        [ConfigurationProperty("pulldays", IsRequired = true)]
        public string PullDays
        {
            get
            {
                return (string)base["pulldays"];
            }
            set
            {
                base["pulldays"] = value.ToString();
            }
        }
    }
}