using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Logic.Transitions
{
    class HpLess : Transition
    {
        private readonly float percent_;

        public HpLess(string targetState, float percent)
            :base(targetState)
        {
            percent_ = percent;    
        }

        protected override bool TryToPerformTransition(Entity target, float dt, ref object state)
        {
            return target.HP / target.Stats.GetFinValue(proto_game.Stats.MaxHealth) <= percent_;
        }
    }
}
