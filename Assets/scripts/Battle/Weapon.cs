using UnityEngine;
using System.Collections.Generic;

public class Weapon : MonoBehaviour 
{
    [SerializeField]
    private BulletSpawnPoint[] bulletSpawnPoints = null;

    [SerializeField]
    private float recoil = 0.0f;

    [SerializeField]
    private proto_game.Weapons weaponType = proto_game.Weapons.MachineGun;

    private Unit owner_;

    public void Init(Unit owner)
    {
        owner_ = owner;
    }


    public float Recoil
    { get { return recoil; } }
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

    public void SpawnBullets(long serverSpawnTime, int firstBulletID)
    {
        var timeElapsedSinceServerSpawn = 0.0f;

        if (serverSpawnTime > -1)
        {
            timeElapsedSinceServerSpawn = owner_.Controller.GameTime - (float)serverSpawnTime / 1000;
        }

        var ownerAttPos = owner_.AttackPosition;
        var ownerAttRot = owner_.AttackDirection;

        float rad = Mathf.Atan2(ownerAttRot.y, ownerAttRot.x);
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        var ownerIsLocalPlayer = owner_.Controller.IsLocalPlayer(owner_);

        foreach(var p in bulletSpawnPoints)
        {
            var bullet = BulletPrefabs.Instance.Get(p.Bullet).GetComponent<Bullet>();

            var spawnPoint = p.transform.localPosition;
            var x = spawnPoint.x * cos - spawnPoint.y * sin;
            var y = spawnPoint.x * sin + spawnPoint.y * cos;
            spawnPoint.x = x + ownerAttPos.x;
            spawnPoint.y = y + ownerAttPos.y;

            //var alpha = owner_.Local ? 0.0f : ((float)timeElapsedSinceServerSpawn/1000.0f) / bullet.TimeAlive;
            var alpha = owner_.Local ? 0.0f : timeElapsedSinceServerSpawn / bullet.TimeAlive;

            if (alpha < 0.9f)
            {
                bullet.Init(owner_.Controller);
                bullet.ID = firstBulletID++;
                bullet.SetOwner(owner_);
                bullet.SetStartPoint(spawnPoint);
                bullet.SetDirection(new Vector3(ownerAttRot.x, ownerAttRot.y, 0) + p.transform.localRotation.eulerAngles);

                //if its local dont move projectile until response from server about move
                bullet.SetMoveSpeed(owner_.Stats.GetFinValue(proto_game.Stats.BulletSpeed));
                bullet.SetDamage(owner_.Stats.GetFinValue(proto_game.Stats.BulletDamage));
                if (alpha > 0.0f)
                    bullet.AdjustPositionToServerTime(alpha);
                bullet.Prepare();

                if (serverSpawnTime > -1)
                    owner_.Controller.Add(bullet);
                else
                    owner_.Controller.AddToScene(bullet);

                if (ownerIsLocalPlayer)
                {
                    bullet.TickOfCreation = owner_.Controller.InputID;
                    //save bullet to start movement after
                    (owner_ as Player).OnBulletShoot(bullet);
                }
                owner_.RegisterBullet(bullet);
            }
            owner_.ApplyRecoil(recoil);
        }
    }
}
