using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle
{
    static class PlayerClasses
    {
        public static void AssaultTank(Player player)
        {
            player.Stats.Get(proto_game.Stats.MaxHealth).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.MaxHealth).SetValue(100.0f);

            player.Stats.Get(proto_game.Stats.MovementSpeed).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.MovementSpeed).SetValue(4.0f);

            player.Stats.Get(proto_game.Stats.BulletDamage).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.BulletDamage).SetValue(10.0f);

            player.Stats.Get(proto_game.Stats.ReloadSpeed).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.ReloadSpeed).SetValue(0.5f);

            player.Stats.Get(proto_game.Stats.BulletSpeed).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.BulletSpeed).SetValue(10.0f);

            player.Stats.Get(proto_game.Stats.Armor).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.Armor).SetValue(0.0f);

            player.Stats.Get(proto_game.Stats.HealthRegen).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.HealthRegen).SetValue(5.0f);
        }

        public static void GunnerTank(Player player)
        {
            player.Stats.Get(proto_game.Stats.MaxHealth).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.MaxHealth).SetValue(80.0f);

            player.Stats.Get(proto_game.Stats.MovementSpeed).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.MovementSpeed).SetValue(4.0f);

            player.Stats.Get(proto_game.Stats.BulletDamage).SetMultiplier(0.7f);
            player.Stats.Get(proto_game.Stats.BulletDamage).SetValue(10.0f);

            player.Stats.Get(proto_game.Stats.ReloadSpeed).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.ReloadSpeed).SetValue(0.45f);

            player.Stats.Get(proto_game.Stats.BulletSpeed).SetMultiplier(0.95f);
            player.Stats.Get(proto_game.Stats.BulletSpeed).SetValue(9.0f);

            player.Stats.Get(proto_game.Stats.Armor).SetMultiplier(0.9f);
            player.Stats.Get(proto_game.Stats.Armor).SetValue(0.0f);

            player.Stats.Get(proto_game.Stats.HealthRegen).SetMultiplier(1.0f);
            player.Stats.Get(proto_game.Stats.HealthRegen).SetValue(5.0f);
        }
    }
}
