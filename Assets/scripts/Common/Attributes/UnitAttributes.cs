using System.Collections.Generic;

namespace Attributes
{
    public class UnitAttributes : AttributeManager<proto_game.Stats>
    {
        public UnitAttributes()
        {
            Add(new Attribute<proto_game.Stats>().Init(proto_game.Stats.MaxHealth));
            Add(new Attribute<proto_game.Stats>().Init(proto_game.Stats.HealthRegen));
            Add(new Attribute<proto_game.Stats>().Init(proto_game.Stats.BulletDamage));
            Add(new Attribute<proto_game.Stats>().Init(proto_game.Stats.BulletSpeed));
            Add(new Attribute<proto_game.Stats>().Init(proto_game.Stats.MovementSpeed));
            Add(new Attribute<proto_game.Stats>().Init(proto_game.Stats.ReloadSpeed));
            Add(new Attribute<proto_game.Stats>().Init(proto_game.Stats.SkillDamage));
            Add(new Attribute<proto_game.Stats>().Init(proto_game.Stats.Armor));
        }
    }
}