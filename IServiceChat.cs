using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceModel;

namespace wcf_chat
{
    [ServiceContract(CallbackContract = typeof(IServerChatCallback))]
    public interface IServiceChat
    {
        [OperationContract]
        int Connect(string name, string password);

        [OperationContract]
        void Disconnect(int id);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string msg, int id);

        [OperationContract(IsOneWay = true)]
        void SendUserList();

        [OperationContract]
        void saveUser(int id, string avatar);

        [OperationContract]
        string getUserAvatar(string name);

        [OperationContract]
        bool getUserConnect(string name);
    }

    public interface IServerChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void MessageCallback(string msg);

        [OperationContract(IsOneWay = true)]
        void UserConnectedCallback(List<string> userList, List<string> userAvatars);
    }

}
