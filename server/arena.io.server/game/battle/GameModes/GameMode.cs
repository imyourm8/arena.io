using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.GameModes
{
    class GameMode
    {
        public Game Game
        { get; set; }

        public virtual void SpawnPlayer(Player player) { }
        public virtual int GetMatchDuration() { return 0; }
        public virtual void HandleEntityKill(Player killer, Entity victim) { }
        public virtual int GetExpFor(Player killer, Player victim) { return 0; }
        public virtual int GetCoinsFor(Player killer, Player victim) { return 0; }
        public virtual int GetScoreFor(Player killer, Player victim) { return 0; }
        public virtual void Update(float dt) { }
        public virtual string GetMapPath() { return ""; }
    }
}
