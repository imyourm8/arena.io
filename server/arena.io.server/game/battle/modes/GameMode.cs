using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.modes
{
    class GameMode
    {
        public Game Game
        { get; set; }

        public virtual void SpawnPlayer(Player player) { }
        public virtual int MaxPlayersAllowed { get; }
        public virtual int GetMatchDurationMs { get; }
        public virtual int CloseGameAfterMs { get; }
        public virtual void HandleEntityKill(Player killer, Entity victim) { }
        public virtual int GetExpFor(Player killer, Player victim) { return 0; }
        public virtual int GetCoinsFor(Player killer, Player victim) { return 0; }
        public virtual int GetScoreFor(Player killer, Player victim) { return 0; }
        public virtual void Update(float dt) { }
        public virtual string MapFilePath { get; }
        public int GetMapID() { return map.MapIDs.MapNameToID[MapFilePath]; }
    }
}
