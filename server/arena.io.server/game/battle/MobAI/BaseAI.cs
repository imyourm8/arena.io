using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using arena.helpers;

namespace arena.battle.MobAI
{
    class BaseAI
    {
        public Mob Owner
        { get; set; }

        private Entity target_;

        public virtual void Update(float dt)
        {
            if (target_ == null)
            {
                SearchForTarget();
            }


            if (target_ != null)
            {
                var distance = MathHelper.Distance(Owner.Position, target_.Position);

                if (distance <= Owner.Entry.AttackRange)
                {
                    Attack();
                }
                else if (distance < Owner.Entry.ReturnRange)
                {
                    Chase(dt);
                }
                else
                {
                    target_ = null;
                    ReturnToSpawnPoint();
                }
            }
        }

        private void SearchForTarget()
        {
            //just pick random first target with agro radius
            foreach (var e in Owner.Game.Map.HitTest(Owner.Position, Owner.Entry.AgroRange))
            {
                if (e is Player)
                {
                    target_ = e as Entity;
                    break;
                }
            }
        }

        protected void Attack()
        {
            var dir = target_.Position - Owner.Position;
            float direction = (float)Math.Atan2(dir.y, dir.x);
            Owner.PerformAttackAtDirection(direction);
        }

        protected void Chase(float dt)
        {
            var dir = target_.Position - Owner.Position;
            Owner.MoveInDirection(dir);
        }

        protected void ReturnToSpawnPoint()
        { }
    }
}
