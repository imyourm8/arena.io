using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : Entity 
{
    [SerializeField]
    private proto_game.Weapons weaponUsed;

    protected Skill skill_;
    protected Weapon weapon_;
    private HashSet<Bullet> firedBullets_ = new HashSet<Bullet>();

    public proto_game.Weapons WeaponUsed
    { get { return weaponUsed; } set { weaponUsed = value; } }

    public Vector2 RecoilVelocity
    { get; set; }

    private float nextAttack_ = 0;
    public float AutoAttackCooldown
    {
        get { return Stats.GetFinValue(proto_game.Stats.ReloadSpeed); }
    }

    public float AttackRange
    { get; set; }

    public override void Init(arena.ArenaController controller)
    {
        base.Init(controller);

        firedBullets_.Clear();

        if (weapon_ != null)
        {
            weapon_.gameObject.ReturnPooled();
        }

        var weapon = WeaponPrefabs.Instance.Get(WeaponUsed);
        var weaponScript = weapon.GetComponent<Weapon>();
        weaponScript.Init(this);
        gameObject.AddChild(weapon);
        weapon_ = weaponScript;
        AttackRange = 0.0f;
        DisableVelocityMovement = true;
    }

    public virtual void ApplyRecoil(float recoil)
    {}

    public bool CanCast()
    {
        return skill_.CanCast();
    }

    public void CastSkill(long serverSpawnTime = -1, int firstBulletID = -1)
    {
        skill_.Cast(serverSpawnTime, firstBulletID);
    }

    public void SetSkillCooldown()
    {
        skill_.Cast();
    }

    public void SetSkill(proto_game.Skills skillId)
    {
        skill_ = SkillsPrefabs.Instance.GetSkill(skillId);
        skill_.Owner = this;

        var cd = Stats.GetFinValue(proto_game.Stats.SkillCooldown);
        skill_.Cooldown = (long)cd;
    }

    public bool CanAttack()
    {
        return Time.time >= nextAttack_;
    }

    public void SetAttackCooldown()
    {
        nextAttack_ = Time.time + AutoAttackCooldown;
    }

    public void RegisterBullet(Bullet bullet)
    {
        firedBullets_.Add(bullet);
    }

    public void UnregisterBullet(Bullet bullet)
    {
        firedBullets_.Remove(bullet);
    }

    public bool PerformAttack(float direction, long time = -1, int firstBulletID = -1)
    {
        Rotation = direction;

        if (weapon_ != null)
        {
            weapon_.SpawnBullets(time, firstBulletID);
        }

        return true;
    }

    public bool PerformAttack(Vector2 direction)
    {
        return PerformAttack((float)Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (skill_ != null)
        {
            skill_.gameObject.ReturnPooled();
            skill_ = null;
        }
    }

    protected override void OnBeforeRemove()
    {
        base.OnBeforeRemove();

        foreach(var bullet in firedBullets_)
        {
            Controller.OnBulletExpired(bullet);
        }

        firedBullets_.Clear();
    }
}
