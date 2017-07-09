using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using arena.battle.Logic.States;
using arena.battle.Logic.Behaviours;
using arena.battle.Logic.Transitions;

namespace arena.Factories
{
    partial class MobScriptsFactory
    {
        private _ SimpleThings = () => Behav()
            .Init(proto_game.MobScriptType.SimpleShooterMobScript,
            new State("idle",
                new Follow(7.0f, 2.0f, 6.0f),
                new RandomMove(0.3f, 0.5f, true),
                new AnyPlayerInRange("charge", 4.0f),
                new State("charge", 
                    new Charge(10.0f, 4.0f, 1.0f),
                    new TimedTransition("attack", 1.0f)
                ),
                new State("attack",
                    new ShootWithWeapon(4.0f, predictive: 6.0f),
                    new NoPlayerInRange("idle", 8.0f)
                )
            ))
            .Init(proto_game.MobScriptType.SimpleBossScript,
            new State("idle",
                new Follow(10.0f, 0.0f, 3.0f),
                new RandomMove(1.0f, 1.5f, true),
                new StayCloseToSpawn(7.0f),
                new AnyPlayerInRange("phase1", 15.0f),
                new State("phase1",
                    new Shoot(15.0f, 6.0f, 10, 36, 1, damage: 70, bulletType: proto_game.Bullets.MonSlowBullet, fixedAngle: 0),
                    new Shoot(15.0f, 6.0f, 10, 36, 1, damage: 70, bulletType: proto_game.Bullets.MonSlowBullet, cooldownOffset: 0.1f, angleOffset:10, fixedAngle:0),
                    new Shoot(15.0f, 6.0f, 10, 36, 1, damage: 70, bulletType: proto_game.Bullets.MonSlowBullet, cooldownOffset: 0.2f, angleOffset: 20, fixedAngle: 0),
                    new Shoot(15.0f, 6.0f, 10, 36, 1, damage: 70, bulletType: proto_game.Bullets.MonSlowBullet, cooldownOffset: 0.3f, angleOffset: 30, fixedAngle: 0),
                    new Shoot(15.0f, 6.0f, 10, 36, 1, damage: 70, bulletType: proto_game.Bullets.MonSlowBullet, cooldownOffset: 0.4f, angleOffset: 40, fixedAngle: 0),
                    new Shoot(15.0f, 6.0f, 10, 36, 1, damage: 70, bulletType: proto_game.Bullets.MonSlowBullet, cooldownOffset: 0.5f, angleOffset: 50, fixedAngle: 0),
                    new Shoot(15.0f, 6.0f, 10, 36, 1, damage: 70, bulletType: proto_game.Bullets.MonSlowBullet, cooldownOffset: 0.6f, angleOffset: 60, fixedAngle: 0),
                    new Shoot(6.0f, 1.0f, shootCount:1, angleStep:0, predictive:3, damage:60, bulletType: proto_game.Bullets.SimpleBossProjectile1, cooldownOffset: 0.2f),
                    new Shoot(6.0f, 3.0f, shootCount:3, angleStep:20, predictive:3, damage:45, bulletType: proto_game.Bullets.SimpleBoosProjectile2, cooldownOffset: 1.5f)
                ),
                new HpLess("phase2", 0.6f),
                new State("phase2",
                    new Shoot(10.0f, 6.0f, shootCount: 10, angleStep: 36, predictive:1, damage: 70, bulletType: proto_game.Bullets.MonSlowBullet),
                    new Shoot(10.0f, 6.0f, shootCount: 10, angleStep: 36, predictive:1, damage: 70, bulletType: proto_game.Bullets.MonSlowBullet, cooldownOffset: 1.0f, angleOffset: 45)
                )
            ));
    }
}
