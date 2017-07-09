using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arena.battle.Logic.Statuses
{
    abstract class GameStatus : arena.common.battle.IStatus
    {
        public GameStatus()
        {
        }

        public abstract bool Update(float dt);
        

        public abstract void Remove();
        public abstract void Apply();

        public Entity Owner
        {
            protected get; 
            set;
        }
    }
}
