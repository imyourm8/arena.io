using UnityEngine;
using System.Collections;

public class Unit : Entity 
{
    [SerializeField]
    private proto_game.Weapons weaponUsed;

    protected Skill skill_;
    protected Weapon weapon_;

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

    public void CastSkill()
    {
        skill_.Cast();
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
}
