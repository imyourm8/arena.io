using UnityEngine;
using System.Collections;

public class PickUp : Entity
{
    [SerializeField]
    private proto_game.Pickups pickupType;

    public proto_game.Pickups PickupType
    { get { return pickupType; } }
}
