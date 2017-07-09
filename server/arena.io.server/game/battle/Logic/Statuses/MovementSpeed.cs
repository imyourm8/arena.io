using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Attributes;

namespace arena.battle.Logic.Statuses
{
    class MovementSpeed : GameStatusTimed
    {
        private Attribute<proto_game.Stats> movementAttribute_ = new Attribute<proto_game.Stats>();

        public MovementSpeed(float speed, float duration = -1.0f):base(duration)
        {
            movementAttribute_.Init(proto_game.Stats.MovementSpeed, speed);
        }

        public override void Remove()
        {
            Owner.Stats.Get(proto_game.Stats.MovementSpeed).RemoveAttribute(movementAttribute_);
            
        }

        public override void Apply()
        {
            Owner.Stats.Get(proto_game.Stats.MovementSpeed).AddAttribute(movementAttribute_);
        }
    }
}
