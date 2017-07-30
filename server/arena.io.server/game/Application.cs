using System;
using System.IO;

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
using shared.database;
using shared.database.Postgres;
using shared.net;
using shared.factories;

namespace arena
{
    public class Application : ApplicationBase
    {
        private static ILogger log = LogManager.GetCurrentClassLogger(); 
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

            Database.Instance.SetDatabaseImplementation(new PostgresImpl());

            PlayerClassFactory.Instance.Init();
            MobsFactory.Instance.Init();
            WeaponFactory.Instance.Init();
            BulletFactory.Instance.Init();
            ExpBlockFactory.Instance.Init();
            PowerUpFactory.Instance.Init();
            SkillFactory.Instance.Init();
            battle.factories.MobScriptsFactory.Instance.Init();
            PickUpFactory.Instance.Init();
            BoosterFactory.Instance.Init();

            battle.RoomManager.Instance.CreateDebugRoom();
        }

        protected override void TearDown()
        {
            
        }
    }
}
