using System;
using Photon.SocketServer;

namespace shared.net
{
    public class ServerApplication : ApplicationBase
    {
        public Guid ServerId { get; private set; }

        public ServerApplication()
        {
            ServerId = Guid.NewGuid();
        }

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            throw new NotImplementedException();
        }

        protected override void Setup()
        {
            throw new NotImplementedException();
        }

        protected override void TearDown()
        {
            throw new NotImplementedException();
        }
    }
}
