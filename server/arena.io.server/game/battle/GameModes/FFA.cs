using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.helpers;

namespace arena.battle.GameModes
{
    class FFA : GameMode
    {
        private Area centralArea_ = new Area(-20, 20, -20, 20);
        Dictionary<proto_game.ExpBlocks, float> expBlockSpawnChances_ = new Dictionary<proto_game.ExpBlocks, float> 
        { 
            {proto_game.ExpBlocks.Small, 50},
            {proto_game.ExpBlocks.Medium, 25},
            {proto_game.ExpBlocks.Big, 20.5f},
            {proto_game.ExpBlocks.Huge, 10.25f}
        };
        private float wholeAreaSpawnWeight_ = 0;
        private float centerAreaSpawnWeight_ = 0;
        private float timeElapsed_ = 0.0f;

        public FFA()
        {
            wholeAreaSpawnWeight_ += expBlockSpawnChances_[proto_game.ExpBlocks.Small];
            wholeAreaSpawnWeight_ += expBlockSpawnChances_[proto_game.ExpBlocks.Medium];

            centerAreaSpawnWeight_ = wholeAreaSpawnWeight_;
            centerAreaSpawnWeight_ += expBlockSpawnChances_[proto_game.ExpBlocks.Big];
            centerAreaSpawnWeight_ += expBlockSpawnChances_[proto_game.ExpBlocks.Huge];
        }

        public override void SpawnPlayer(Player player)
        {
            base.SpawnPlayer(player);

            Game.Map.SpawnPlayer(player);
        }

        public override int GetMatchDuration()
        {
            return 1500000;
        }

        public override int GetExpFor(Player player)
        {
            int exp = player.BattleStats.Kills * 40;
            exp += 20;
            return exp;
        }

        public override int GetCoinsFor(Player player)
        {
            int gold = player.BattleStats.Gold;
            gold += player.BattleStats.Kills * 10;
            return gold;
        }

        public override string GetMapPath()
        {
            return "maps/ffa";
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            timeElapsed_ += dt;

            if (timeElapsed_ > 500000000)
            { 
                //spawn test boss
                var boss = Mob.Create(proto_game.MobType.SimpleBoss);
                boss.Position = Game.Map.Center;
                Game.Add(boss);
                timeElapsed_ = -3000000;
            }
        }
    }
}
