using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExitGames.Concurrency.Core;

namespace arena
{
    class DebugExecutor : IExecutor
    {
        void IExecutor.Execute(Action toExecute)
        {
            toExecute();
        }

        void IExecutor.Execute(List<Action> toExecute)
        {
            foreach (var action in toExecute)
            {
                action();
            }
        }
    }
}
