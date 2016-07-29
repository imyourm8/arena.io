using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour 
{
    [SerializeField]
    private GameObject[] bulletSpawnPoints = null;

    [SerializeField]
    private float recoil;

    private Entity owner_;

	public void Init(Entity owner)
    {
        owner_ = owner;
    }

    public void SpawnBullets()
    {
        foreach(var p in bulletSpawnPoints)
        {
            var bullet = owner_.Controller.CreateBullet();
            if (bullet == null) 
                continue;

            var attDir = owner_.AttackDirection;
            bullet.Init(owner_, 
                new Vector3(attDir.x, attDir.y, 0) + p.transform.localRotation.eulerAngles, 
            owner_.Stats.GetFinValue(proto_game.Stats.BulletSpeed), 
            p.transform.position, 
            owner_.Stats.GetFinValue(proto_game.Stats.BulletDamage));

            owner_.ApplyRecoil(recoil);
            Physics2D.IgnoreCollision(bullet.Collider, owner_.Collider);
        }
    }
}
