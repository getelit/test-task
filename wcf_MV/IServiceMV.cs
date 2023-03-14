using System.ServiceModel;

namespace wcf_MV
{
    [ServiceContract(CallbackContract = typeof(IServerMVCallback))]
    public interface IServiceMV
    {
        [OperationContract]
        int Connect(string name);

        [OperationContract]
        void Disconnect(int id);

        [OperationContract(IsOneWay = true)]
        void SendMsg(string msg, int id);
    }

    public interface IServerMVCallback
    {
        [OperationContract(IsOneWay = true)]
        void MsgCallback(string msg);
    }
}
