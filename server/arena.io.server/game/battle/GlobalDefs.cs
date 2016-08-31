using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    static class GlobalDefs
    {
        public static readonly int EventPoolInterval = 10;
        public static readonly int MainTickInterval = 100;
        public static float GetUpdateInterval()
        {
            return (float)MainTickInterval / 1000.0f;
        }

        public static readonly int AITickInterval = 100;
        public static float GetAIUpdateInterval()
        {
            return (float)AITickInterval / 1000.0f;
        }

        public static readonly int BroadcastEntitiesInterval = 200;
    }
}
