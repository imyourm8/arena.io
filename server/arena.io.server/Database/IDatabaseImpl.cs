using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.Database
{
    interface IDatabaseImpl
    {
        IAuthDB AuthDB
        {
            get;
        }

        IPlayerDB PlayerDB
        {
            get;
        }

        ILootDB LootDB
        {
            get;
        }

        ICreatureDB CreatureDB
        {
            get;
        }

        IWorldDB WorldDB
        {
            get;
        }

        ISpellDB SpellDB
        {
            get;
        }

        string[] ExtractStringArray(object arrValue);
    }
}
