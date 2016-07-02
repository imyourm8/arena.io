using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.MobAI
{
    class BaseAI
    {
        public virtual void Update(float dt)
        { }

        public Mob Owner
        { get; set; }
    }
}
