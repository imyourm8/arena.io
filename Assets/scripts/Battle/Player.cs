using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class Player : Entity, PlayerExperience.IExpProvider 
{
    private static readonly float defaultTightness = 0.06f;
    private static readonly float smoothTightness = 0.025f;

    [SerializeField]
    private Vector3 nicknameOffset = Vector3.zero;

    [SerializeField]
    private proto_game.Weapons weaponUsed;

    [SerializeField]
    private proto_profile.PlayerClasses plrClass;

    private Vector2 desiredPosition_;
    private PlayerExperience playerExp_;
    private Text nicknameText_ = null;
    private StatusManager statusManager_;
    private Tween moveTween_;
    private float tightness_ = defaultTightness;
    private Nito.Deque<Vector2> localMoves_ = new Nito.Deque<Vector2>();

	public Player()
	{
        playerExp_ = new PlayerExperience(this);
        statusManager_ = new StatusManager(this);
	}
    #if UNITY_EDITOR 
    public proto_profile.PlayerClasses @Class
    { get { return plrClass; } }
    #endif
    public bool Replaying
    { get; set; }

    public proto_game.PlayerInput.Request Input
    { get; set; }

    public proto_game.Shoot Shoot
    { get; set; }

    public proto_game.Weapons WeaponUsed
    { get { return weaponUsed; } set { weaponUsed = value; } }

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

    public bool Smoothing 
    { get { return tightness_ < defaultTightness-0.1f; } }

    public PlayerExperience PlayerExperience
    {
        get { return playerExp_; }
    }

    public override void Init(arena.ArenaController controller, Vector2 startPos)
    {
        base.Init(controller, startPos);
        Input = new proto_game.PlayerInput.Request();
        Shoot = new proto_game.Shoot();
        Level = 1;
        Replaying = false;
        var weapon = WeaponPrefabs.Instance.Get(WeaponUsed);
        var weaponScript = weapon.GetComponent<Weapon>();
        weaponScript.Init(this);
        gameObject.AddChild(weapon);
        weapon_ = weaponScript;
        desiredPosition_ = startPos;
    }

    public void OnPowerUpGrabbed(PowerUp powerUp)
    {
        Controller.OnPowerUpGrabbed(this, powerUp);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        statusManager_.Update();

        var prevPosition = Position;
        if (!Replaying && (Smoothing || moveTween_ == null))
        {
            Position = prevPosition + (desiredPosition_ - prevPosition) * tightness_;
        }

        tightness_ += (defaultTightness - tightness_) * 0.01666f;
    }

    public void LateUpdate()
    {
        if (Local)
        {
            UpdateHpBarPosition();    
        }

        if (nicknameText_ != null)
        {
            nicknameText_.transform.position = transform.localPosition + nicknameOffset;
        }
    }

    public void CancelMoving()
    {
        if (moveTween_ != null)
            moveTween_.Kill();
    }

    public void Snap(float x, float y)
    {
        desiredPosition_.x = x;
        desiredPosition_.y = y;
    }

    public override void OnFixedUpdate(float dt)
    {
        var totalImpulse = Velocity;
        totalImpulse.x *= dt;
        totalImpulse.y *= dt;
        desiredPosition_ += totalImpulse;

        if (!Replaying && !Smoothing)
        {
            CancelMoving();
            moveTween_ = DOTween.To(()=>Position,x=>Position=x,desiredPosition_,GameApp.Instance.MovementUpdateDT);
            moveTween_.OnComplete(HandleMoveTweenComplete);
        }

        base.OnFixedUpdate(dt);
    }

    public void Smooth()
    {
        tightness_ = smoothTightness;
        localMoves_.Clear();
    }

    public void SetPosition(float x, float y)
    {
        Position = new Vector2(x, y);
        desiredPosition_ = new Vector2(x, y);
    }

    public override PhysicalState GetState()
    {
        if (!Local) return base.GetState();

        PhysicalState state = new PhysicalState();
        state.Position = desiredPosition_;
        state.Velocity = Velocity;
        return state;
    }

    public override void OnRemove()
    {
        base.OnRemove();
        CancelMoving();
        if (weapon_ != null)
            weapon_.gameObject.ReturnPooled();
        if (nicknameText_ != null)
            nicknameText_.gameObject.ReturnPooled();
    }

    public void AddStatus(proto_game.PowerUpType effectType, float duration)
    {
        Status statusEffect = new Status(effectType, duration);
        statusManager_.Add(statusEffect);
    }

    private void MoveTo(Vector2 pos)
    {
        moveTween_ = DOTween.To(()=>Position,x=>Position=x,desiredPosition_,GameApp.Instance.MovementUpdateDT);
        moveTween_.OnComplete(HandleMoveTweenComplete);
    }

    private void HandleMoveTweenComplete()
    {
        moveTween_ = null;
    }
}
