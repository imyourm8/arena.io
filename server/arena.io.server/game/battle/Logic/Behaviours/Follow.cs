using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.logic.behaviours
{
    class Follow : StatusBehaviour
    {
        class FollowState
        {
            public float duration;
            public Entity target;
        }

        private readonly float acquireRange_;
        private readonly float minRange_;
        private readonly float duration_;

        public Follow(float acquireRange, float minRange, float duration, float speed = -1)
        {
            acquireRange_ = acquireRange;
            minRange_ = minRange;
            duration_ = duration;
        }

        protected override void OnEnter(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        {
            FollowState state = null;
            if (behaviorState == null)
            {
                state = new FollowState();
            }
            else
            {
                state = (behaviorState as FollowState);
            }

            state.target = null;
            behaviorState = state;
        }

        protected override void HandleUpdate(Entity target, float dt, ref object state)
        {
            FollowState followState = state as FollowState;

            if (followState.target == null)
            {
                followState.target = target.Game.Map
                    .GetNearestEntities(target.Position, acquireRange_, PhysicsDefs.Category.PLAYER).FirstOrDefault() as Player;
                followState.duration = duration_;

                Status = BehaviourStatus.Started;
            }

            if (followState.target != null)
            {
                followState.duration -= dt;

                var dir = followState.target.Position - target.Position;

                if (dir.Length() > minRange_)
                {
                    target.MoveInDirection(dir);
                    target.SetRotation(dir);
                }

                if (followState.duration <= 0.0f)
                {
                    followState.target = null;
                    Status = BehaviourStatus.Finished;
                }
            }
        }
    }
}
