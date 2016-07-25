using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class Map
    {
        private PowerUpLayer powerUps_;
        private ExpLayer expLayer_;
        private SpatialHash entityHash_;
        private BlockSpawner expSpawner_;
        private PlayerSpawnsLayer playerSpawns_;

        public Map(
            Game game, 
            PowerUpLayer powerUps, 
            ExpLayer expLayer, 
            NavigationLayer navLayer, 
            PlayerSpawnsLayer playerSpawns)
        {
            powerUps_ = powerUps;
            expLayer_ = expLayer;
            playerSpawns_ = playerSpawns;
            entityHash_ = new SpatialHash(navLayer);
            expSpawner_ = new BlockSpawner(expLayer, game);
            powerUps.Game = game;
        }

        public void SpawnPlayer(Player player)
        {
            var spawnPoint = playerSpawns_.GetSpawnPoint();
            player.X = spawnPoint.Item1;
            player.Y = spawnPoint.Item2;
        }

        public void Update()
        {
            expSpawner_.Update();
            powerUps_.Update();
        }

        public void Add(Entity entity)
        {
            entityHash_.Add(entity);
        }

        public void Remove(Entity entity)
        {
            entityHash_.Remove(entity);
        }

        public void Clear()
        {
            entityHash_.Clear();
        }

        public void Move(Entity entity, float x, float y)
        {
            entityHash_.Move(entity, x, y);
        }

        public void OnExpBlockRemoved(Entity entity)
        {
            expSpawner_.OnExpBlockRemoved(entity);
        }
    }
}
