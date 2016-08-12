using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using arena.battle;

namespace arena.Factories
{
    class PowerUpFactory : TapCommon.Singleton<PowerUpFactory>
    {
        private Dictionary<proto_game.PowerUpType, PowerUpEntry> powerUps_ = new Dictionary<proto_game.PowerUpType, PowerUpEntry>();
        public void Init()
        {
            var jsonPowerUps = JArray.Parse(File.ReadAllText("game_data/power_ups_export.json"));

            foreach (var puData in jsonPowerUps)
            {
                var entry = new PowerUpEntry(puData);
                powerUps_.Add(entry.Type, entry);
            }
        }

        public PowerUpEntry GetEntry(proto_game.PowerUpType type)
        {
            return powerUps_[type];
        }
    }
}
