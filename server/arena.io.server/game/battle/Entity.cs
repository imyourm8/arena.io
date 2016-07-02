using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class Entity
    {
        private Attributes.UnitAttributes stats_ = new Attributes.UnitAttributes();
        public Attributes.UnitAttributes Stats
        { get { return stats_; }}

        public int ID
        { get; set; }

        public float X
        { get; set; }

        public float Y
        { get; set; }

        public float HP
        { get; set; }

        public int Exp
        { get; set; }

        public proto_game.UnitDie GetDiePacket()
        {
            proto_game.UnitDie diePck = new proto_game.UnitDie();
            diePck.guid = ID;
            return diePck;
        }

        public proto_game.PlayerStats GetStatsPacket()
        {
            proto_game.PlayerStats stats = new proto_game.PlayerStats();

            foreach (var stat in Stats)
            {
                var s = new proto_game.StatValue();
                s.multiplier = stat.Multipler;
                s.stat = stat.ID;
                s.value = stat.FinalValue;
                stats.stats.Add(s);
            }

            return stats;
        }

        public virtual void Update(float dt)
        { }
    }
}
