using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    class Mob : Entity
    {
        private MobAI.BaseAI ai_;
        public MobAI.BaseAI AI
        {
            set { ai_ = value; }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            ai_.Update(dt);
        }
    }
}
