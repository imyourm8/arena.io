using System;
using Photon.SocketServer;

namespace shared.net
{
    public class ServerApplication : ApplicationBase
    {
        public ServerApplication()
        {
            ServerId = Guid.NewGuid();
        }

        #region Properties

        public Guid ServerId { get; private set; }
        public string Ip { get; private set; }

        #endregion

        #region ApplicationBase implemtation

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

        #endregion
    }
}
