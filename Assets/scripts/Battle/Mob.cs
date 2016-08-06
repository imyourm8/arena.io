using UnityEngine;
using System.Collections;

public class Mob : Entity 
{
    [SerializeField]
    private proto_game.MobType type;

    public proto_game.MobType Type
    { get { return type; } }
}
