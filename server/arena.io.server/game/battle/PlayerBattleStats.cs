using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class PlayerBattleStats
    {
        public int Score
        { get; set; }

        public int Coins
        { get; set; }

        public int Kills
        { get; set; }

        public int FlagsCaptured
        { get; set; }

        public int DistanceTraveled
        { get; set; }

        public void Reset()
        {
            Score = 0;
            Kills = 0;
            FlagsCaptured = 0;
            Coins = 0;
        }
    }
}
