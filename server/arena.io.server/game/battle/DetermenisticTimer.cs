using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class DetermenisticTimer
    {
        private long interval_ = 0;
        private long accumulator_ = 0;
        private Action action_ = null;
        private List<Action> delayedActions_ = new List<Action>();

        public DetermenisticTimer(long interval)
        {
            interval_ = interval;
        }

        public void OnElapsed(Action action)
        {
            action_ = action;
        }

        public void Update(long dt)
        {
            accumulator_ += dt;
            while (accumulator_ >= interval_)
            {
                accumulator_ -= interval_;
                action_();
                foreach (var action in delayedActions_)
                {
                    action();
                }
                delayedActions_.Clear();
            }
        }

        public void DelayAction(Action action)
        {
            delayedActions_.Add(action);
        }
    }
}
