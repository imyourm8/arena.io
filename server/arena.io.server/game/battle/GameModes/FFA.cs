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
            return player.Highscore > 0 ? 100 : 0;
        }

        public override int GetCoinsFor(Player player)
        {
            return player.Highscore > 0 ? 100 : 0;
        }

        public override string GetMapPath()
        {
            return "maps/ffa";
        }
    }
}
