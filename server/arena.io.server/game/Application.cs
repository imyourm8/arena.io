using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using Photon.SocketServer.Concurrency;
using Photon.SocketServer.Operations;
using Photon.SocketServer.ServerToServer;
using Photon.SocketServer.Security;
using Photon.SocketServer.Numeric;
using System.IO;

using ExitGames.Concurrency.Core;
using ExitGames.Concurrency.Fibers;
using ExitGames.Diagnostics.Monitoring;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

using log4net.Config;
using arena.player;

namespace arena
{
    public class Application : Photon.SocketServer.ApplicationBase
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

            Database.Database.Instance.SetDatabaseImplementation(new Database.Postgres.PostgresImpl());

            Factories.PlayerClassFactory.Instance.Init();
            Factories.MobsFactory.Instance.Init();
            Factories.WeaponFactory.Instance.Init();
            Factories.BulletFactory.Instance.Init();
            Factories.ExpBlockFactory.Instance.Init();
            Factories.PowerUpFactory.Instance.Init();
            Factories.SkillFactory.Instance.Init();
            Factories.MobScriptsFactory.Instance.Init();
            Factories.PickUpFactory.Instance.Init();

            battle.RoomManager.Instance.CreateDebugRoom();
        }

        protected override void TearDown()
        {
            
        }
    }
}
