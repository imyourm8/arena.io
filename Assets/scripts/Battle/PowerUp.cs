using UnityEngine;
using System.Collections;

public class PowerUp : Entity 
{
    [SerializeField]
    private proto_game.PowerUpType Type;

    public proto_game.PowerUpType PowerUpType
    { get { return Type; }}

    public float RemoveAfter
    {
        get; set;
    }
}
