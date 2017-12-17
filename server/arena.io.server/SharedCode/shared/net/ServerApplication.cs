using System;
using Photon.SocketServer;
using ExitGames.Concurrency.Fibers;

namespace shared.net
{
    public class ServerApplication : ApplicationBase
    {
        private IFiber executionFiber_;

        public ServerApplication()
        {
            ServerId = Guid.NewGuid();
        }

        #region Properties

        public Guid ServerId { get; private set; }
        public string Ip { get; private set; }

        #endregion

        #region ApplicationBase implementation

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            throw new NotImplementedException();
        }

        protected override void Setup()
        {
            executionFiber_ = new PoolFiber();
            executionFiber_.Start();
        }

        protected override void TearDown()
        {
            
        }

        #endregion

        #region Public Methods

        public void Execute(Action action)
        {
            executionFiber_.Enqueue(action);
        }

        public void Schedule(Action action, int delayMs)
        {
            executionFiber_.Schedule(action, delayMs);
        }

        public void ScheduleOnInterval(Action action, int delayMs, int intervalMs)
        {
            executionFiber_.ScheduleOnInterval(action, delayMs, intervalMs);
        }

        #endregion
    }
}
