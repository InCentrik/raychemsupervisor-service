using System.ServiceModel;

namespace IC.RCS.RCSCore
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IRCSFormWCFService
    {
        [OperationContract]
        void LogForm(string message);
    }


}
