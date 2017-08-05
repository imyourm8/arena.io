using System;
using System.IO;
using System.Collections.Generic;
using System.Data;

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

namespace LoginServer
{
    public class Application : ApplicationBase
    {
        private static ILogger log = LogManager.GetCurrentClassLogger(); 

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            PlayerConnection connection = new PlayerConnection(initRequest);
            connection.SetController(new LoginController());
            return connection;  
        }

        protected override void Setup()
        { 
            log4net.GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(this.ApplicationPath, "log");
            var configFileInfo = new FileInfo(Path.Combine(this.BinaryPath, "log4net.config"));
            if (configFileInfo.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(configFileInfo);
            }
        }

        protected override void TearDown()
        {

        }
    }
}
