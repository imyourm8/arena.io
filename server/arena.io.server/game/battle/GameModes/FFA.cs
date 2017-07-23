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

        private float timeElapsed_ = 0.0f;

        public FFA()
        {
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

        public override int GetExpFor(Player killer, Player victim)
        {
            var lvlDiff = System.Math.Max(1, victim.Level - killer.Level);
            return 10 * victim.Level * lvlDiff;
        }

        public override int GetCoinsFor(Player killer, Player victim)
        {
            return victim.Level / 10;
        }

        public override int GetScoreFor(Player killer, Player victim)
        {
            return victim.BattleStats.Score / 10;
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
                Game.Add(boss);
                timeElapsed_ = -3000000;
            }
        }
    }
}
