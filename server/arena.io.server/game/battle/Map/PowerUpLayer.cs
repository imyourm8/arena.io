using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using shared.factories;
using shared.helpers;

namespace arena.battle
{
    class PowerUpLayer
    {
        private List<PowerUpSpawnPoint> spawnPoints_;
        private PowerUp currentPowerUp_;
        private long prevSpawnTime_ = 0;

        public PowerUpLayer(List<PowerUpSpawnPoint> points)
        {
            spawnPoints_ = points;
            prevSpawnTime_ = CurrentTime.Instance.CurrentTimeInMs;
        }

        public Game Game
        { get; set; }

        public long RespawnDelay
        { get; set; }

        public void Update()
        {
            if (currentPowerUp_ != null && currentPowerUp_.Lifetime <= 0)
            {
                currentPowerUp_ = null;
                prevSpawnTime_ = CurrentTime.Instance.CurrentTimeInMs;
            }

            if (currentPowerUp_ == null && CurrentTime.Instance.CurrentTimeInMs - prevSpawnTime_ >= RespawnDelay)
            {
                var point = spawnPoints_[MathHelper.Range(0, spawnPoints_.Count - 1)];

                var powerUp = new PowerUp();

                float x, y;
                point.Area.RandomPoint(out x, out y);
                powerUp.SetPosition(x, y);
                powerUp.Type = helpers.Extensions.PickRandom<proto_game.PowerUpType>(point.Probabilities, point.TotalWeight);
                var entry = PowerUpFactory.Instance.GetEntry(powerUp.Type);
                powerUp.PickupType = entry.PickUpType;
                powerUp.Radius = entry.CollisionRadius;
                powerUp.Lifetime = point.Durations[powerUp.Type];
                powerUp.Game = Game;
                Game.AddPowerUp(powerUp);

                currentPowerUp_ = powerUp;
                prevSpawnTime_ = CurrentTime.Instance.CurrentTimeInMs;
            }
        }
    }
}
