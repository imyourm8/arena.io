using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.helpers;

namespace arena.battle.logic.behaviours
{
    class StayCloseToSpawn : Behaviour
    {
        class StayCloseToSpawnState
        {
            public Vector2 spawnPosition;
        }

        private readonly float maxRadius_;

        public StayCloseToSpawn(float maxRadius)
        {
            maxRadius_ = maxRadius;
        }

        protected override void OnEnter(Entity holder, IStateStorage stateHolder, ref object behaviorState)
        {
            var state = new StayCloseToSpawnState();
            state.spawnPosition = holder.Position;
            behaviorState = state;
        }

        protected override void HandleUpdate(Entity target, float dt, ref object state)
        {
            StayCloseToSpawnState scState = state as StayCloseToSpawnState;

            if (MathHelper.Distance(scState.spawnPosition, target.Position) > maxRadius_)
            {
                var backwardDir = scState.spawnPosition - target.Position;
                target.MoveInDirection(backwardDir);
            }
        }
    }
}
