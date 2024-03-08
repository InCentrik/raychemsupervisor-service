using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data;

namespace IC.RCS.RCSCore
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IRCSWCFService
    {

        [OperationContract]
        string GetData(int value);

        [OperationContract]
        string GetExePath();

        [OperationContract]
        bool TestSQLCredentials(string serverName, string databaseName, string username, string password);

        [OperationContract]
        void PullTrendGroupsFromSQL(string serverName, string databaseName, string username, string password);

        [OperationContract]
        void StartTrendGroupTransfer();
    }


}
