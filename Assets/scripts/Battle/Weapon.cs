using UnityEngine;
using System.Collections.Generic;

public class Weapon : MonoBehaviour 
{
    [SerializeField]
    private BulletSpawnPoint[] bulletSpawnPoints = null;

    [SerializeField]
    private float recoil;

    [SerializeField]
    private proto_game.Weapons weaponType;

    private Entity owner_;

	public void Init(Entity owner)
    {
        owner_ = owner;
    }

    #if UNITY_EDITOR
    public List<BulletSpawnPoint> GetSpawnPoints()
    {
        return new List<BulletSpawnPoint>(bulletSpawnPoints);
    }

    public proto_game.Weapons GetWeaponType()
    {
        return weaponType;
    }
    #endif

    public void SpawnBullets()
    {
        var ownerAttPos = owner_.AttackPosition;
        var ownerAttRot = owner_.AttackDirection;

        float rad = Mathf.Atan2(ownerAttRot.y, ownerAttRot.x);
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        foreach(var p in bulletSpawnPoints)
        {
            var bullet = owner_.Controller.CreateBullet();
            if (bullet == null) 
                continue;

            var position = p.transform.localPosition + new Vector3(ownerAttPos.x, ownerAttPos.y, 0);

            var point = p.transform.localPosition;
            var x = point.x * cos - point.y * sin;
            var y = point.x * sin + point.y * cos;
            point.x = x + ownerAttPos.x;
            point.y = y + ownerAttPos.y;

            bullet.Init(
                owner_, 
                new Vector3(ownerAttRot.x, ownerAttRot.y, 0) + p.transform.localRotation.eulerAngles, 
                owner_.Stats.GetFinValue(proto_game.Stats.BulletSpeed), 
                point, 
                owner_.Stats.GetFinValue(proto_game.Stats.BulletDamage)
            );

            owner_.ApplyRecoil(recoil);
            owner_.Controller.AddBullet(bullet);
            //dont allow collide with owned bullets
            Physics2D.IgnoreCollision(bullet.Collider, owner_.Collider);
        }
    }
}
