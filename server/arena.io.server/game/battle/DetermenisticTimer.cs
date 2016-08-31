using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class DetermenisticTimer : IDisposable
    {
        private long interval_ = 0;
        private long accumulator_ = 0;
        private Action<long> action_ = null;
        private List<Action> delayedActions_ = new List<Action>();

        public DetermenisticTimer(long interval)
        {
            interval_ = interval;
        }

        public long Interval
        { get { return interval_; } }

        public void OnElapsed(Action<long> action)
        {
            action_ = action;
        }

        public long GetAccumulator()
        {
            return accumulator_;
        }

        public void Update(long dt)
        {
            accumulator_ += dt;
            while (accumulator_ >= interval_)
            {
                accumulator_ -= interval_;
                if (action_ != null)
                {
                    action_(interval_);
                    foreach (var action in delayedActions_)
                    {
                        action();
                    }
                    delayedActions_.Clear();
                }
            }
        }

        public void DelayAction(Action action)
        {
            delayedActions_.Add(action);
        }

        public void Dispose()
        {
            action_ = null;
            delayedActions_.Clear();
        }
    }
}
