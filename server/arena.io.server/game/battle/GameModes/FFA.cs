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

            var wholeArea = Game.Size;
            player.X = MathHelper.Range(wholeArea.minX, wholeArea.maxX);
            player.Y = MathHelper.Range(wholeArea.minY, wholeArea.maxY);
        }

        public override proto_game.ExpBlocks GetBlockTypeByPoint(float x, float y)
        {
            float blockWeight = centralArea_.IsInside(x, y) ? centerAreaSpawnWeight_ : wholeAreaSpawnWeight_;
            float blockRoll = MathHelper.Range(0, blockWeight);
            float totalRoll = 0;
            proto_game.ExpBlocks type = proto_game.ExpBlocks.Small;
            foreach (var blockSpawn in expBlockSpawnChances_)
            {
                if (blockRoll > totalRoll)
                {
                    type = blockSpawn.Key;
                }

                if (totalRoll > blockRoll) break;

                totalRoll += blockSpawn.Value;
            }
            return type;
        }

        public override int GetMatchDuration()
        {
            return 150000;
        }

        public override int GetExpFor(Player player)
        {
            return player.Highscore > 0 ? 100 : 0;
        }

        public override int GetCoinsFor(Player player)
        {
            return player.Highscore > 0 ? 100 : 0;
        }
    }
}
