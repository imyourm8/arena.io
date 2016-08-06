using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using arena.helpers;

namespace arena.battle
{
    class SpatialHash
    {
        public interface IEntity
        {
            Vector2 Position { get; }
            void SetPosition(float x, float y);
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
            var bucket = GetBucket(entity.Position);
            bucket.TryAdd(entity.ID, entity);
        }

        public void Remove(IEntity entity)
        {
            RemoveFromBucket(entity);
        }

        public void Move(IEntity entity, Vector2 pos)
        {
            var entityPos = entity.Position;
            var currentTile = GetTileCoord(entityPos);
            var newTile = GetTileCoord(pos);

            if (newTile != currentTile)
            {
                RemoveFromBucket(entity);

                var bucket = GetBucket(entityPos);
                bucket.TryAdd(entity.ID, entity);
            }

            entity.SetPosition(pos.x, pos.y);
        }

        public IEnumerable<IEntity> HitTest(Vector2 pos, float radius)
        {
            int xl = (int)(pos.x - radius) / width_;
            int xh = (int)(pos.x + radius) / width_;
            int yl = (int)(pos.y - radius) / height_;
            int yh = (int)(pos.y + radius) / height_;
            for (var x_ = xl; x_ <= xh; x_++)
                for (var y_ = yl; y_ <= yh; y_++)
                {
                    ConcurrentDictionary<int, IEntity> bucket;
                    if (hash_.TryGetValue(Hash(x_, y_), out bucket))
                        foreach (var i in bucket) yield return i.Value;
                }
        }

        private void RemoveFromBucket(IEntity entity)
        {
            var bucket = GetBucket(entity.Position);
            bucket.TryRemove(entity.ID, out entity);
        }

        private ConcurrentDictionary<int, IEntity> GetBucket(Vector2 pos)
        {
            int hash = GetTileCoord(pos);
            ConcurrentDictionary<int, IEntity> bucket = hash_.GetOrAdd(hash, _ => new ConcurrentDictionary<int, IEntity>());    
            return bucket;
        }

        private int GetTileCoord(Vector2 pos)
        {
            int tx = (int)Math.Floor(pos.x / (float)width_);
            tx = tx >= width_ ? width_ : tx;
            int ty = (int)Math.Floor(pos.y / (float)height_);
            ty = ty >= height_ ? height_ : ty;
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
