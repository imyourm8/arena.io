using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

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
            prevSpawnTime_ = helpers.CurrentTime.Instance.CurrentTimeInMs;
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
                prevSpawnTime_ = helpers.CurrentTime.Instance.CurrentTimeInMs;
            }

            if (currentPowerUp_ == null && helpers.CurrentTime.Instance.CurrentTimeInMs - prevSpawnTime_ >= 5000)
            {
                var point = spawnPoints_[helpers.MathHelper.Range(0, spawnPoints_.Count - 1)];

                var powerUp = new PowerUp();

                float x, y;
                point.Area.RandomPoint(out x, out y);

                powerUp.X = x;
                powerUp.Y = y;
                powerUp.Type = helpers.Extensions.PickRandom<proto_game.PowerUpType>(point.Probabilities, point.TotalWeight);
                powerUp.Lifetime = point.Durations[powerUp.Type];

                Game.AddPowerUp(powerUp);

                currentPowerUp_ = powerUp;
                prevSpawnTime_ = helpers.CurrentTime.Instance.CurrentTimeInMs;
            }
        }
    }
}
