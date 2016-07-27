using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace arena.battle
{
    public class SpatialHash
    {
        public interface IEntity
        {
            float X { get; set; }
            float Y { get; set; }
            int ID { get; }
        }

        private int width_, height_ = 0;
        private NavigationLayer navLayer_;
        private ConcurrentDictionary<int, ConcurrentDictionary<int, IEntity>> hash_
            = new ConcurrentDictionary<int, ConcurrentDictionary<int, IEntity>>();

        public SpatialHash(NavigationLayer navLayer)
        {
            width_ = navLayer.TileWidth;
            height_ = navLayer.TileHeight;
        }

        public void Add(IEntity entity)
        {
            var bucket = GetBucket(entity.X, entity.Y);
            bucket.TryAdd(entity.ID, entity);
        }

        public void Remove(IEntity entity)
        {
            RemoveFromBucket(entity);
        }

        public void Move(IEntity entity, float x, float y)
        {
            var currentTile = GetTileCoord(entity.X, entity.Y);
            var newTile = GetTileCoord(x, y);

            if (newTile != currentTile)
            {
                RemoveFromBucket(entity);
                entity.X = x;
                entity.Y = y;

                var bucket = GetBucket(entity.X, entity.Y);
                bucket.TryAdd(entity.ID, entity);
            }
        }

        public IEnumerable<IEntity> HitTest(float x, float y, float radius)
        {
            int xl = (int)(x - radius) / width_;
            int xh = (int)(x + radius) / width_;
            int yl = (int)(y - radius) / height_;
            int yh = (int)(y + radius) / height_;
            for (var x_ = xl; x_ <= xh; x++)
                for (var y_ = yl; y_ <= yh; y++)
                {
                    ConcurrentDictionary<int, IEntity> bucket;
                    if (hash_.TryGetValue(Hash(x_, y_), out bucket))
                        foreach (var i in bucket) yield return i.Value;
                }
        }

        private void RemoveFromBucket(IEntity entity)
        {
            var bucket = GetBucket(entity.X, entity.Y);
            bucket.TryRemove(entity.ID, out entity);
        }

        private ConcurrentDictionary<int, IEntity> GetBucket(float x, float y)
        {
            int hash = GetTileCoord(x, y);
            ConcurrentDictionary<int, IEntity> bucket = hash_.GetOrAdd(hash, _ => new ConcurrentDictionary<int, IEntity>());    
            return bucket;
        }

        private int GetTileCoord(float x, float y)
        {
            int tx = (int)Math.Floor(x / (float)width_);
            tx = tx >= width_ ? width_ : tx;
            int ty = (int)Math.Floor(y / (float)height_);
            ty = ty >= height_ ? height_ : tx;
            return Hash(tx, ty);
        }

        private int Hash(int x, int y)
        {
            return (x << 16) | y;
        }

        public void Clear()
        {
            foreach (var c in hash_)
            {
                c.Value.Clear();
            }

            hash_.Clear();
        }
    }
}
