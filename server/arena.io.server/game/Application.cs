using System;
using System.IO;
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

using arena.net;
using arena.serv;
using arena.serv.load_balancing;
using shared.database;
using shared.database.Postgres;
using shared.net;
using shared.factories;

namespace arena
{
    public class Application : ServerApplication
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();

        private GameNodeController serverController_;

#region ApplicationBase implementation
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            PlayerConnection connection = new PlayerConnection(initRequest);
            connection.SetController(new PlayerController());
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

            var loginServerIp = Path.Combine(ApplicationPath, "lobby_server_ip");  
            if (File.Exists(loginServerIp))
            {
                var ip = File.ReadAllText(loginServerIp);       
                MasterServerConnection connection = new MasterServerConnection(this, ip, Ports.LobbyPort, 500);
                serverController_ = new GameNodeController(this);
                connection.SetController(serverController_); 
                connection.Connect();  
            }
            else
            {
                log.FatalFormat("Can't connect to login server. File {0} is not exist.", loginServerIp);
            }

            WorkloadController = new WorkloadController(this, "GameNode", 1000, workLoadConfigFile: "");

            Database.Instance.SetDatabaseImplementation(new PostgresImpl());

            PlayerClassFactory.Instance.Init(ApplicationPath);
            MobsFactory.Instance.Init(ApplicationPath);
            WeaponFactory.Instance.Init(ApplicationPath);
            BulletFactory.Instance.Init(ApplicationPath);
            ExpBlockFactory.Instance.Init(ApplicationPath);
            PowerUpFactory.Instance.Init(ApplicationPath);
            SkillFactory.Instance.Init(ApplicationPath);
            PickUpFactory.Instance.Init(ApplicationPath);
            BoosterFactory.Instance.Init();

            battle.factories.MobScriptsFactory.Instance.Init();
        }

        protected override void TearDown()
        {

        }
#endregion

        public WorkloadController WorkloadController
        {
            get;
            set;
        }
    }
}
