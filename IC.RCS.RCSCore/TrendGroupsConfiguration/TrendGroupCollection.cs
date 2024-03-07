using System.Configuration;

namespace IC.RCS.RCSCore
{
    public class TrendGroupCollection : ConfigurationElementCollection
    {
        // Create a property that lets us access an element in the
        // collection with the int index of the element
        public TrendGroupElement this[int index]
        {
            get
            {
                // Gets the TrendGroupElement at the specified
                // index in the collection
                return (TrendGroupElement)BaseGet(index);
            }
            set
            {
                // Check if a TrendGroupElement exists at the
                // specified index and delete it if it does
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);

                // Add the new TrendGroupElement at the specified
                // index
                BaseAdd(index, value);
            }
        }

        // Create a property that lets us access an element in the
        // colleciton with the name of the element
        public new TrendGroupElement this[string key]
        {
            get
            {
                // Gets the TrendGroupElement where the name
                // matches the string key specified
                return (TrendGroupElement)BaseGet(key);
            }
            set
            {
                // Checks if a TrendGroupElement exists with
                // the specified name and deletes it if it does
                if (BaseGet(key) != null)
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));

                // Adds the new TrendGroupElement
                BaseAdd(value);
            }
        }

        // Method that must be overriden to create a new element
        // that can be stored in the collection
        protected override ConfigurationElement CreateNewElement()
        {
            return new TrendGroupElement();
        }

        // Method that must be overriden to get the key of a
        // specified element
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TrendGroupElement)element).Guid;
        }
        public void Add(TrendGroupElement element)
        {
            LockItem = false;
            BaseAdd(element);
        }
    }
}