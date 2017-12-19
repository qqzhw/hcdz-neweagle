using System.Collections.Generic;
using System.Text;
using Pvirtech.TcpSocket.Scs.Communication.Messages;
using Pvirtech.TcpSocket.Scs.Communication.Protocols;

namespace Pvirtech.QyRound.ViewModels
{
    internal class CustomWireProtocol : IScsWireProtocol
    {

        public IEnumerable<IScsMessage> CreateMessages(byte[] receivedBytes)
        {
            return new List<IScsMessage>
            {
                new ScsRawDataMessage(receivedBytes)
            };
        }
   
        public IEnumerable<IScsMessage> CreateMessages(string receicedMsg)
        {
            var bytes = Commons.CommonHelper.HexStringToByteArray(receicedMsg);
            return new List<IScsMessage>
            { 
                new ScsRawDataMessage(bytes)
            };
        }

        public byte[] GetBytes(IScsMessage message)
        {
            if (message is ScsPingMessage)
            {
                return new byte[1];
            }
            return ((ScsRawDataMessage)message).MessageData;
        }

        public void Reset()
        {
            
        }
    
  
        
    }
}