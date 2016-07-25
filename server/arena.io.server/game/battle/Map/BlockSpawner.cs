using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.helpers;

namespace arena.battle
{
    class BlockSpawner
    {
        public interface IBlockControl
        {
            void AddBlock(ExpBlock entity);
            void RemoveBlock(ExpBlock entity);
        }

        private IBlockControl control_;
        private int width_, height_;
        private int maxBlocks_;
        private float cellWidth_, cellHeight_;
        private helpers.Area spawnArea_;
        private ConcurrentDictionary<int, ConcurrentDictionary<int, int>> buckets_ 
                = new ConcurrentDictionary<int, ConcurrentDictionary<int, int>>();
        private int unitsPerCell_;
        private ExpLayer layer_;

        public BlockSpawner(ExpLayer layer, IBlockControl control)
        {
            layer_ = layer;
            control_ = control;
            width_ = layer.TileWidth;
            height_ = layer.TileHeight;
            maxBlocks_ = layer.MaxBlocks;
            spawnArea_ = layer.Area;  
            unitsPerCell_ = (int)Math.Ceiling((float)maxBlocks_ / (float)(width_ * height_));
            cellWidth_ = spawnArea_.Width / (float)width_;
            cellHeight_ = spawnArea_.Height / (float)height_;

            for (int i = 0; i < width_; ++i)
            {
                var bucket = new ConcurrentDictionary<int, int>();
                buckets_.TryAdd(i, bucket);
                for (int j = 0; j < height_; ++j)
                    bucket.TryAdd(j, 0);
            }
        }

        public void OnExpBlockRemoved(Entity entity)
        {
            if (entity is ExpBlock)
            {
                control_.RemoveBlock(entity as ExpBlock);

                var tile = GetTileCoord(entity.X - spawnArea_.minX, entity.Y - spawnArea_.minY);

                ConcurrentDictionary<int, int> bucket;
                buckets_.TryGetValue(tile.Key, out bucket);

                int count = 0;
                bucket.TryGetValue(tile.Value, out count);
                bucket.TryUpdate(tile.Value, count - 1, count);
            }
        }

        public void Update()
        {
            foreach (var bucket in buckets_) 
            {
                float x = bucket.Key ;
                foreach (var p in bucket.Value) 
                {
                    float y = p.Key;
                    var countToSpawn = unitsPerCell_ - p.Value;
                    for (var ii = 0; ii < countToSpawn; ++ii)
                    {
                        SpawnBlock(spawnArea_.minX + x * cellWidth_, 
                            spawnArea_.minX + (x + 1) * cellWidth_,
                            spawnArea_.minY + y * cellHeight_,
                            spawnArea_.minY + (y + 1) * cellHeight_);
                    }

                    if (countToSpawn > 0)
                        bucket.Value.TryUpdate(p.Key, p.Value + countToSpawn, p.Value);
                }
            }
        }

        private KeyValuePair<int, int> GetTileCoord(float x, float y)
        {
            int tx = (int)Math.Floor(x / cellWidth_);
            tx = tx >= width_ ? width_ : tx;
            int ty = (int)Math.Floor(y / cellHeight_);
            ty = ty >= height_ ? height_ : ty;
            return new KeyValuePair<int, int>(tx, ty); 
        }

        private void SpawnBlock(float minX, float maxX, float minY, float maxY)
        {
            var block = new ExpBlock();
            block.X = MathHelper.Range(minX, maxX);
            block.Y = MathHelper.Range(minY, maxY);

            var type = layer_.GetBlockTypeByPoint(block.X, block.Y);
            int exp = 0;
            int coins = 0;
            float hp = 0;

            switch (type)
            {
                case proto_game.ExpBlocks.Small:
                    exp = 10;
                    hp = 25;
                    break;

                case proto_game.ExpBlocks.Medium:
                    exp = 80;
                    hp = 55;
                    break;

                case proto_game.ExpBlocks.Big:
                    exp = 200;
                    hp = 220;
                    break;

                case proto_game.ExpBlocks.Huge:
                    exp = 500;
                    hp = 560;
                    break;

                case proto_game.ExpBlocks.GoldBlock:
                    exp = 10;
                    hp = 60;
                    coins = helpers.MathHelper.Range(1, 10);
                    break;
            }

            block.BlockType = type;
            block.Exp = exp;
            block.HP = hp;
            block.Coins = coins;
            block.Stats.SetValue(proto_game.Stats.MaxHealth, hp);

            control_.AddBlock(block);
        }
    }
}
