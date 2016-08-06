﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.battle;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.Factories
{
    class WeaponFactory : TapCommon.Singleton<WeaponFactory> 
    {
        private Dictionary<proto_game.Weapons, WeaponEntry> weapons_ = new Dictionary<proto_game.Weapons, WeaponEntry>();

        public void Init()  
        { 
            var jsonWeapons = JArray.Parse(File.ReadAllText("game_data/weapon_export.json"));

            foreach (JToken wepData in jsonWeapons)
            {
                var entry = new WeaponEntry(wepData);
                weapons_.Add(entry.Type, entry);
            }
        }

        public WeaponEntry GetEntry(proto_game.Weapons type)
        {
            return weapons_[type];
        }
    }
}
