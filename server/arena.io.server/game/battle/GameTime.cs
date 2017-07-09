using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    public interface IGameTimeReader
    {
        int Tick
        { get; }

        long RealTime
        { get; }
    }

    class GameTime : IGameTimeReader
    {
        public long RealTime
        { get; set; }

        public int Tick
        { get; set; }
    }
}
