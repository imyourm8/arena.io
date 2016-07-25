using UnityEngine;
using System.Collections;

public class PowerUp : Entity 
{
    [SerializeField]
    private proto_game.PowerUpType Type;

    public float RemoveAfter
    {
        get; set;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Entity entity = other.gameObject.GetComponent<Entity>();
        Player plr = entity as Player;
        if (plr != null)
        {
            Status statusEffect = new Status(RemoveAfter);
            plr.Add(statusEffect);
        }
    }
}
