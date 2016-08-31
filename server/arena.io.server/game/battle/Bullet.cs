﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Box2DX.Dynamics;
using Box2DX.Common;

using arena.helpers;

namespace arena.battle
{
    class Bullet : Entity
    {
        private float timeElapsed_ = 0.0f;

        public Bullet()
        {
            Category = PhysicsDefs.Category.BULLET;
            TrackSpatially = false;
        }

        public Unit Owner 
        { get; set; }

        public BulletEntry Entry
        { get; set; }

        public override void InitPhysics(bool dynamicBody = true, bool isSensor = false)
        {
            return; //TODO trying client side hit detection
            base.InitPhysics(dynamicBody, isSensor);
            ushort mask = (ushort)PhysicsDefs.Category.PLAYER;
            switch ((ushort)Owner.Category)
            {
                case (ushort)PhysicsDefs.Category.PLAYER:
                    mask |= (ushort)PhysicsDefs.Category.MOB | (ushort)PhysicsDefs.Category.EXP_BLOCK;
                    break;
            }
            AddToCollisionMask(mask);
            timeElapsed_ = 0;
        }

        public void OnCollision(Entity target)
        {
            if (target == null || target == Owner)  
            {
                return; 
            }

            target.ApplyDamage(Owner, Stats.GetFinValue(proto_game.Stats.BulletDamage));

            if (!Entry.Penetrate)
            {
                SelfDestroy();
            }
        }

        public override void Update(float dt)
        {
            timeElapsed_ += dt;

            if (timeElapsed_ >= Entry.TimeAlive)
            {
                SelfDestroy();
            }
        }

        private void SelfDestroy()
        {
            Game.Remove(this);
        }
    }
}
