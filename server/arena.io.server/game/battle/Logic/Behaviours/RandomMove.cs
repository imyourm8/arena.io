using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;

namespace arena.battle.logic.behaviours
{
    /*
     * Choose random direction, moves by Random(minMove, maxMove), then repeat cycle
     */
    class RandomMove : Behaviour
    {
        class RandomMoveState
        {
            public float moveLeft;
        }

        private readonly float minMove_;
        private readonly float maxMove_;
        private readonly bool additive_;

        public RandomMove(float minMove, float maxMove, bool additive = true)
        {
            minMove_ = minMove;
            maxMove_ = maxMove;
            additive_ = additive;
        }

        protected override void OnEnter(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        {
            var state = new RandomMoveState();
            state.moveLeft = 0.0f;
            behaviorState = state;
        }

        protected override void HandleUpdate(Entity target, float dt, ref object state)
        {
            RandomMoveState moveState = state as RandomMoveState;
            if (moveState.moveLeft <= 0.0f)
            {
                moveState.moveLeft = MathHelper.Range(minMove_, maxMove_);
                var dir = new Vector2(MathHelper.Range(-1.0f, 1.0f), MathHelper.Range(-1.0f, 1.0f));
                if (additive_)
                {
                    var curDir = target.Velocity;
                    curDir.Normilize();

                    dir += curDir;
                    dir.Normilize();
                }
                target.MoveInDirection(dir);
            }
            else
            {
                moveState.moveLeft -= dt;
            }
        }
    }
}
