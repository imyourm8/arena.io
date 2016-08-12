using UnityEngine;
using System.Collections;

public class Mob : Unit 
{
    [SerializeField]
    private proto_game.MobType type = proto_game.MobType.Basic;

    public proto_game.MobType Type
    { get { return type; } }
}
