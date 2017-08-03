using System.Net;

using Photon.SocketServer;

using shared.net;

namespace arena.serv
{
    class MasterServerConnection : ServerConnection
    {
        public MasterServerConnection(ApplicationBase application, string masterIp, int port, int connectRetryIntervalSeconds)
            : base(application, masterIp, port, connectRetryIntervalSeconds)
        {
        }

        public ServerRequestHandler Controller { private get; set; }

        protected override void OnConnectionEstablished(object responseObject)
        { 
            
        }


        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        { }
    }
}
