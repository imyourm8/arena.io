using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Net;

using Photon.SocketServer;
using Photon.SocketServer.Concurrency;
using Photon.SocketServer.Operations;
using Photon.SocketServer.ServerToServer;
using Photon.SocketServer.Security;
using Photon.SocketServer.Numeric;

using ExitGames.Concurrency.Core;
using ExitGames.Concurrency.Fibers;
using ExitGames.Diagnostics.Monitoring;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

using log4net.Config;

using shared.net;

namespace LobbyServer
{
    using controller;
    using load_balancing;
    using matchmaking;

    public class Application : ServerApplication
    {
        #region Fields

        private static ILogger log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Properties

        internal Loadbalancer Loadbalancer { get; private set; }
        internal RemoteGameManager GameManager { get; private set; }

        #endregion

        #region ApplicationBase implementation

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            PeerBase connectionPeer = null;    
            if (IsGameNodeConnection(initRequest))
            {
                InboundServerConnection connection = new InboundServerConnection(this, initRequest);
                ServerController controller = new GameNodeController(this);
                connection.SetController(controller);
                connectionPeer = connection;
            }
            else
            {
                PlayerConnection connection = new PlayerConnection(initRequest);
                connection.SetController(new LobbyController(this));
                connectionPeer = connection;
            }
            return connectionPeer;
        }

        protected override void Setup()
        {
            base.Setup();

            log4net.GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(this.ApplicationPath, "log");
            var configFileInfo = new FileInfo(Path.Combine(this.BinaryPath, "log4net.config"));
            if (configFileInfo.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(configFileInfo);
            }

            /*var loginServerIp = Path.Combine(this.ApplicationPath, "login_server_ip");
            if (File.Exists(loginServerIp))
            {
                var ipstring = File.ReadAllText(loginServerIp);
                var ip = IPAddress.Parse(ipstring);
                serverController_ = new ServerController(this);
                if (!serverController_.ConnectTcp(new IPEndPoint(ip, Ports.LoginPort), "LobbyServer"))
                {
                    log.FatalFormat("Can't connect to login server with {0} address.", ipstring);
                }
            }
            else
            {
                log.FatalFormat("Can't connect to login server. File {0} is not exist.", loginServerIp);
            }*/

            Loadbalancer = new Loadbalancer();
            GameManager = new RemoteGameManager(this);
        }

        protected override void TearDown()
        {
            
        }

        #endregion

        #region Private Methods

        private bool IsGameNodeConnection(InitRequest request)
        {
            return request.LocalPort == Ports.LobbyPort; 
        }

        #endregion
    }
}
