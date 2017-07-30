using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;

namespace arena.battle.logic.behaviours
{
    class ShootWithWeapon : Behaviour
    {
        class ShootWithWeaponState
        {
            public float delay;
        }

        private readonly float angle_;
        private readonly float delay_;
        private readonly float radius_;
        private readonly float predict_;

        public ShootWithWeapon(float radius, float delay = 0, float angle = -1, float predictive = 0)
        {
            angle_ = angle * MathHelper.Deg2Rad;
            delay_ = delay;
            radius_ = radius;
            predict_ = predictive;
        }

        protected override void OnEnter(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        {
            var state = new ShootWithWeaponState();
            state.delay = delay_;
            behaviorState = state;
        }

        protected override void OnExit(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        {
            behaviorState = null;
        }

        protected override void HandleUpdate(Entity target, float dt, ref object state)
        {
            var unit = target as Unit;
            if (unit == null)
            {
                return;
            }

            var shootState = state as ShootWithWeaponState;

            if (shootState.delay <= 0.0f)
            {
                var shootTarget = unit.Game.Map
                    .GetNearestEntities(unit.Position, radius_, PhysicsDefs.Category.PLAYER).FirstOrDefault();
                if (shootTarget != null)
                {
                    var angle = angle_;
                    if (angle < 0.0f)
                    {
                        var direction = shootTarget.Position - target.Position;
                        angle = (float)Math.Atan2(direction.y, direction.x);
                    }

                    if (predict_ > 0.0f)
                    {
                        angle += Shoot.Predict(unit, shootTarget) * predict_;  
                    }

                    unit.PerformAttackAtDirection(angle);
                    unit.Rotation = angle;
                }
            }
            else 
            {
                shootState.delay -= dt;
            }
        }
    }
}
