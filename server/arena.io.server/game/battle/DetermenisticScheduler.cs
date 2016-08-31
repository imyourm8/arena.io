using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExitGames.Logging;
using ExitGames.Logging.Log4Net;

namespace arena.battle
{
    class DetermenisticScheduler
    {
        private static ILogger log = LogManager.GetCurrentClassLogger();

        class Task
        {
            private Action action_;
            public long interval_;
            public int runs_ = 0;

            public Task(Action action, long interval)
            {
                action_ = action;
                interval_ = interval; 
            }

            public void Run(long accumulator)
            {
                var nextRun = runs_ * interval_ + interval_;
                if (nextRun <= accumulator)
                {
                    action_();
                    runs_++;
                }
            }
        }

        private List<Task> tasks_ = new List<Task>();
        private long accumulator_ = 0;
        private long runStep_ = long.MaxValue;
        private long runStepAccumulated_ = 0;
         
        public void ScheduleOnInterval(Action action, long interval)
        {
            tasks_.Add(new Task(action, interval));
        }

        public void SetStep(long step)
        {
            runStep_ = step;
        }

        public long AccumulatedTime
        {
            get { return accumulator_; }
        }

        public void Update(long dt)
        {
            runStepAccumulated_ += dt;
            while (runStepAccumulated_ >= runStep_) 
            {
                accumulator_ += runStep_;
                foreach (var task in tasks_) 
                {
                    task.Run(accumulator_);
                }
                runStepAccumulated_ -= runStep_;
            }
        }
    }
}
