using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Box2CS;

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
        private World world_;
        private Game game_;

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
            game_ = game;

            world_ = navLayer.World;  
            world_.ContactListener = new GameContactListener();
            world_.ContactFilter = new GameContactFilter();
        }

        public Body CreateBody(BodyDef def)
        {
            return world_.CreateBody(def);
        }

        public void SpawnPlayer(Player player)
        {
            var spawnPoint = playerSpawns_.GetSpawnPoint();
            player.SetPosition(spawnPoint.Item1, spawnPoint.Item2);
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

        public void StepPhysics(float dt)
        {
            world_.Step(dt, 2, 2);
        }

        public void Remove(Entity entity)
        {
            if (entity.TrackSpatially)
                entityHash_.Remove(entity);

            game_.Execute(() => 
            {
                if (entity.Body != null)
                {
                    world_.DestroyBody(entity.Body);
                    entity.Body = null;
                }
            });
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

        public IEnumerable<Entity> GetNearestEntities(Vector2 pos, float radius)
        {
            foreach (var e in HitTest(pos, radius))
            {
                var newDist = helpers.MathHelper.Distance(pos, e.Position);
                if (newDist < radius)
                {
                    yield return e as Entity;
                }
            }
            yield break;
        }

        public IEnumerable<Entity> GetNearestEntities(Vector2 pos, float radius, PhysicsDefs.Category category)
        {
            foreach (var e in HitTest(pos, radius))
            {
                if (e.Category != category) continue;
                if (helpers.MathHelper.Distance(pos, e.Position) < radius)
                {
                    yield return e as Entity;
                }
            }
            yield break;
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
