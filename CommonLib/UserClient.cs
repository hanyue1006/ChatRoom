using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CommonLib
{
    public enum ProtocolKind
    {
        BroadCast = 1,
        SingleChat = 2,
        MultChat = 3,
        FileTrans = 4,
        BigFileTrans = 5
    }
    public class UserClient
    {
        public IPAddress ip { get; set; }
        public int port { get; set; }
        public IPEndPoint ipep { get; set; }
        public Socket sokConnection { get; set; }
        public UserClient(Socket s)
        {
            this.sokConnection = s;
            this.ipep=(IPEndPoint)sokConnection.RemoteEndPoint;
            this.ip = ipep.Address;
            this.port = ipep.Port;
        }
    }
}
