using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using ExitGames.Logging;
using ExitGames.Concurrency.Fibers;

namespace shared.net
{
    public class MasterServerConnection : OutboundServerConection
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private string masterIp_;
        private int port_;
        private int connectRetryIntervalMs_;
        private IPEndPoint endpoint_;
        private IFiber fiber_;
        private IDisposable reconnectTimer_;
        private long isReconnecting_ = 0;

        public MasterServerConnection(ServerApplication app, string masterIp, int port, int connectRetryIntervalMs)
            :base(app)
        {
            connectRetryIntervalMs_ = connectRetryIntervalMs;
            masterIp_ = masterIp;
            port_ = port;

            fiber_ = new PoolFiber();
            fiber_.Start();
        }

        #region Public Methods

        public void Connect()
        {
            if (reconnectTimer_ != null)
            {
                reconnectTimer_.Dispose();
                reconnectTimer_ = null;
            }

            // check if the photon application is shuting down
            if (!Application.Running)
            {
                return;
            }

            try
            {
                UpdateEndpoint();

                if (log.IsDebugEnabled)
                    log.DebugFormat("MasterServer endpoint for address {0} updated to {1}", this.masterIp_, this.endpoint_);

                if (ConnectTcp(endpoint_, Application.ApplicationName))
                {
                    if (log.IsInfoEnabled)
                        log.InfoFormat("Connecting to master at {0}, serverId={1}", endpoint_, Application.ServerId);
                }
                else
                {
                    if (log.IsWarnEnabled)
                        log.WarnFormat("Connection refused - is the process shutting down ? {0}", this.Application.ServerId);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                if (isReconnecting_ == 1)
                {
                    Reconnect();
                }
                else
                {
                    throw;
                }
            }
        }

        public void UpdateEndpoint()
        {
            IPAddress masterAddress;
            if (!IPAddress.TryParse(masterIp_, out masterAddress))
            {
                var hostEntry = Dns.GetHostEntry(this.masterIp_);
                if (hostEntry.AddressList == null || hostEntry.AddressList.Length == 0)
                {
                    throw new ExitGames.Configuration.ConfigurationException(
                        "MasterIPAddress setting is neither an IP nor an DNS entry: " + this.masterIp_);
                }

                masterAddress = hostEntry.AddressList.First(address => address.AddressFamily == AddressFamily.InterNetwork);

                if (masterAddress == null)
                {
                    throw new ExitGames.Configuration.ConfigurationException(
                        "MasterIPAddress does not resolve to an IPv4 address! Found: "
                        + string.Join(", ", hostEntry.AddressList.Select(a => a.ToString()).ToArray()));
                }
            }

            endpoint_ = new IPEndPoint(masterAddress, port_);
        }

        protected void Reconnect()
        {
            if (!Application.Running)
            {
                return;
            }

            reconnectTimer_ = fiber_.ScheduleOnInterval(() => Connect(), connectRetryIntervalMs_, connectRetryIntervalMs_);
            Interlocked.Exchange(ref isReconnecting_, 1);
        }

        #endregion

        #region OutboundS2SPeer overrides

        protected override void OnConnectionEstablished(object responseObject)
        {
            base.OnConnectionEstablished(responseObject);

            Interlocked.Exchange(ref isReconnecting_, 0);
        }

        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        {
            base.OnConnectionFailed(errorCode, errorMessage);

            if (isReconnecting_ == 0)
            {
                log.ErrorFormat(
                    "Connection failed: address={0}, errorCode={1}, msg={2}",
                    endpoint_,
                    errorCode,
                    errorMessage);
            }
            else if (log.IsWarnEnabled)
            {
                log.WarnFormat(
                    "Master connection failed: address={0}, errorCode={1}, msg={2}",
                    endpoint_,
                    errorCode,
                    errorMessage);
            }

            Reconnect();
        }

        #endregion
    }
}
