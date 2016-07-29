using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Player : Entity, PlayerExperience.IExpProvider 
{
    [Serializable]
    public enum WeaponType
    {
        Starting
    }

    [SerializeField]
    private WeaponDict weaponsPrefabs = null;

    [SerializeField]
    private Vector3 nicknameOffset;

    private PlayerExperience playerExp_;
    private WeaponType weaponUsed_ = WeaponType.Starting;
    private Text nicknameText_ = null;
    private StatusManager statusManager_;

	public Player()
	{
        playerExp_ = new PlayerExperience(this);
        statusManager_ = new StatusManager(this);
	}

    public Text NicknameText
    { set { nicknameText_ = value; }}

    private string nickname_ = null;
    public string Nickname
    {
        set { nickname_ = value; nicknameText_.text = nickname_;}
        get { return nickname_; }
    }

    public int Level
    { get; set; }

    public int AttackCooldownLevel
    { get; set; }

    public PlayerExperience PlayerExperience
    {
        get { return playerExp_; }
    }

    public override void Init(arena.ArenaController controller, Vector2 startPos)
    {
        base.Init(controller, startPos);

        Level = 1;

        var prefab = weaponsPrefabs[weaponUsed_];
        var weapon = Instantiate<GameObject>(prefab); 
        var weaponScript = weapon.GetComponent<Weapon>();
        weaponScript.Init(this);
        gameObject.AddChild(weapon);
        weapon_ = weaponScript;
    }

    public void OnPowerUpGrabbed(PowerUp powerUp)
    {
        Controller.OnPowerUpGrabbed(this, powerUp);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (nicknameText_ != null)
        {
            nicknameText_.transform.position = transform.localPosition + nicknameOffset;
        }

        statusManager_.Update();
    }

    public override void OnRemove()
    {
        base.OnRemove();

        nicknameText_.gameObject.ReturnPooled();
    }

    public void AddStatus(proto_game.PowerUpType effectType, float duration)
    {
        Status statusEffect = new Status(effectType, duration);
        statusManager_.Add(statusEffect);
    }
}
