using System.Configuration;

namespace IC.RCS.RCSCore
{
    public class TrendGroupConfig : ConfigurationSection
    {
        // Create a property that lets us access the collection
        // of TrendGroupElements

        // Specify the name of the element used for the property
        [ConfigurationProperty("trendGroups")]
        // Specify the type of elements found in the collection
        [ConfigurationCollection(typeof(TrendGroupCollection))]
        public TrendGroupCollection TrendGroups
        {
            get
            {
                // Get the collection and parse it
                return (TrendGroupCollection)this["trendGroups"];
            }
            set { this["trendGroups"] = value; }
        }
    }
}
