using UnityEngine;
using System.Collections;

public class Unit : Entity 
{
    [SerializeField]
    private proto_game.Weapons weaponUsed;

    public proto_game.Weapons WeaponUsed
    { get { return weaponUsed; } set { weaponUsed = value; } }

    public override void Init(arena.ArenaController controller, Vector2 startPos)
    {
        base.Init(controller, startPos);

        var weapon = WeaponPrefabs.Instance.Get(WeaponUsed);
        var weaponScript = weapon.GetComponent<Weapon>();
        weaponScript.Init(this);
        gameObject.AddChild(weapon);
        weapon_ = weaponScript;
    }
}
