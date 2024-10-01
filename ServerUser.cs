using System.Runtime.Serialization;
using System.ServiceModel;

namespace wcf_chat
{
    [DataContract]
    public class ServerUser
    {
        [DataMember]
        public int ID { get; set; }
        
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Avatar { get; set; }

        [DataMember]
        public bool isConnected { get; set; }
        
        [DataMember]
        public OperationContext operationContext { get; set; }
    }
}
