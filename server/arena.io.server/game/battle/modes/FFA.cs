using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.helpers;
using arena.battle.map;

namespace arena.battle.modes
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

        public override int GetMatchDurationMs()
        {
            return 300000;
        }

        public override int CloseGameAfterMs()
        {
            return GetMatchDurationMs() - 60000; //1 min before end, close game
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
            return MapIDs.FFA_1;
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
