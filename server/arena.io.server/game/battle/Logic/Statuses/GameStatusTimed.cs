using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Logic.Statuses
{
    class GameStatusTimed : GameStatus
    {
        private float timeElapsed_ = 0.0f;
        private float removeAfter_ = 0.0f;

        public GameStatusTimed(float removeAfter):base()
        {
            removeAfter_ = removeAfter;
        }

        public override bool Update(float dt)
        {
            if (removeAfter_ <= 0.0f) return false;

            timeElapsed_ += dt;
            return timeElapsed_ >= removeAfter_;
        }

        public override void Remove()
        {
        }

        public override void Apply()
        {
        }
    }
}
