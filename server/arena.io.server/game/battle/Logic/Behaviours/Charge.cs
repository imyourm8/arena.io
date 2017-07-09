using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.helpers;
using arena.battle.Logic.Statuses;

namespace arena.battle.Logic.Behaviours
{
    class Charge : Behaviour
    {
        class ChargeState
        {
            public Vector2 direction;
            public float timeLeft;
        }

        private float speed_ = 0.0f;
        private float acquireRange_ = 0.0f;
        private float duration_ = 0.0f;

        public Charge(float speed, float range, float duration)
        {
            speed_ = speed;
            acquireRange_ = range;
            duration_ = duration;
        }

        protected override void OnEnter(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        {
            ChargeState state = null;
            if (behaviorState == null)
            {
                state = new ChargeState();
            }
            else
            {
                state = (behaviorState as ChargeState);
            }

            state.direction = Vector2.zero;
            state.timeLeft = -1;
            behaviorState = state;
        }

        protected override void HandleUpdate(Entity target, float dt, ref object state)
        {
            ChargeState chargeState = state as ChargeState;

            if (chargeState.timeLeft < 0)
            {
                //find new target
                var chargeTarget = target.Game.Map
                        .GetNearestEntities(target.Position, acquireRange_, PhysicsDefs.Category.PLAYER).FirstOrDefault() as Player;
                chargeState.timeLeft = duration_;
                chargeState.direction = target.Position - chargeTarget.Position;
                chargeState.direction.Normilize();

                target.AddStatus(new MovementSpeed(speed_, duration_));
            }

            target.MoveInDirection(chargeState.direction);
        }
    }
}
