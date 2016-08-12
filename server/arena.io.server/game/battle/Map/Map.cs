﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.helpers;

namespace arena.battle
{
    class Map
    {
        private PowerUpLayer powerUps_;
        private ExpLayer expLayer_;
        private SpatialHash entityHash_;
        private BlockSpawner expSpawner_;
        private PlayerSpawnsLayer playerSpawns_;
        private NavigationLayer navLayer_;
        private MobLayer mobLayer_;

        public Map(
            Game game, 
            PowerUpLayer powerUps, 
            ExpLayer expLayer, 
            NavigationLayer navLayer, 
            PlayerSpawnsLayer playerSpawns,
            MobLayer mobLayer)
        {
            mobLayer_ = mobLayer;
            powerUps_ = powerUps;
            navLayer_ = navLayer;
            expLayer_ = expLayer;
            playerSpawns_ = playerSpawns;
            entityHash_ = new SpatialHash(navLayer);
            expSpawner_ = new BlockSpawner(expLayer, game);

            powerUps_.Game = game;
            mobLayer_.Game = game;
        }

        public void SpawnPlayer(Player player)
        {
            var spawnPoint = playerSpawns_.GetSpawnPoint();
            player.SetPosition(spawnPoint.Item1, spawnPoint.Item2);
        }

        public List<float> GetOuterBorder()
        {
            List<float> points = new List<float>();
            var area = navLayer_.OuterBorder;
            points.Add(area.minX);
            points.Add(area.minY);
            points.Add(area.maxX);
            points.Add(area.maxY);
            return points;
        }

        public void Update()
        {
            expSpawner_.Update();
            powerUps_.Update();
            mobLayer_.Update();
        }

        public void Add(Entity entity)
        {
            if (entity.TrackSpatially)
                entityHash_.Add(entity);
        }

        public void Remove(Entity entity)
        {
            if (entity.TrackSpatially)
                entityHash_.Remove(entity);
        }

        public void Clear()
        {
            entityHash_.Clear();
        }

        public void RefreshHashPosition(Entity entity)
        {
            entityHash_.RefreshHashPosition(entity);
        }

        public IEnumerable<SpatialHash.IEntity> HitTest(Vector2 pos, float radius)
        {
            return entityHash_.HitTest(pos, radius);
        }

        public void OnExpBlockRemoved(Entity entity)
        {
            expSpawner_.OnExpBlockRemoved(entity);
        }

        public void OnMobDead(Mob mob)
        {
            mobLayer_.OnMobDead(mob);
        }
    }
}
