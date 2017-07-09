using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using arena.battle.Logic;
using arena.battle.Logic.States;
using arena.battle.Logic.Behaviours;

namespace arena.Factories
{
    partial class MobScriptsFactory : TapCommon.Singleton<MobScriptsFactory>
    {
        private Dictionary<proto_game.MobScriptType, State> behaviours_ = new Dictionary<proto_game.MobScriptType, State>();

        private delegate init _();

        struct init
        {
            public init Init(proto_game.MobScriptType scriptType, State rootState)
            {
                rootState.Init();
                MobScriptsFactory.Instance.behaviours_.Add(scriptType, rootState);
                return this;
            }
        }

        private static init Behav()
        {
            return new init();
        }

        public void Init()
        {
            //Super hacky way to initialize scripts which should be located separately
            FieldInfo[] fields = GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.FieldType == typeof(_))
                .ToArray();
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                ((_)field.GetValue(this))();
            }
        }

        public State Get(proto_game.MobScriptType id)
        {
            return behaviours_[id];
        }
    }
}
