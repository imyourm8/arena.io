using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;

using arena.common.battle;

public class Player : Unit, PlayerExperience.IExpProvider 
{
    private static readonly float SmoothTightness = 0.05f;
    private static readonly float DefaulTightness = 0.1f;

    [SerializeField]
    private Vector3 nicknameOffset = Vector3.zero;

    [SerializeField]
    private proto_profile.PlayerClasses plrClass = proto_profile.PlayerClasses.Assault;

    private Vector2 desiredPosition_;
    private PlayerExperience playerExp_;
    private Text nicknameText_ = null;
    private StatusManager statusManager_;
    private Vector2 previousPos_ = Vector2.zero;
    private float posAlpha_ = 0;
    private float tightness_ = 0;
    private float appliedRecoil_ = 0;

    private GameObject serverGhost_ = null;

	public Player()
	{
        playerExp_ = new PlayerExperience(this);
        statusManager_ = new StatusManager(this);
	}

    public float InterpolationBaseValue
    { get; set; }

    public bool Smoothed
    { get; private set; }

    #if UNITY_EDITOR 
    public proto_profile.PlayerClasses @Class
    { get { return plrClass; } }
    #endif
    public bool Replaying
    { get; set; }

    public proto_game.PlayerInput.Request Input
    { get; set; }

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
       
        Input = new proto_game.PlayerInput.Request();

        Level = 1;
        Replaying = false;
        desiredPosition_ = startPos;
    }

    public override void OnUpdate(float dt)
    {
        statusManager_.Update(dt);

        posAlpha_ = InterpolationBaseValue / GameApp.Instance.MovementUpdateDT;

        if (!Smoothed)
        {
            Position = Vector2.Lerp(previousPos_, desiredPosition_, posAlpha_);
        }
        else
        {
            Position = Position + (desiredPosition_ - Position) * tightness_;
            tightness_ += (DefaulTightness - tightness_) * 0.01666f;

            if ((desiredPosition_-Position).magnitude < 0.01f)
            {
                Smoothed = false;
            }
        }

        base.OnUpdate(dt);

        if (nicknameText_ != null)
        {
            nicknameText_.transform.position = transform.localPosition + nicknameOffset;
        }
    }

    public void CreateServerGhost()
    {
        serverGhost_ = gameObject.GetPooled();
        serverGhost_.GetComponent<Player>().Collider.enabled = false;
        Controller.gameObject.AddChild(serverGhost_);
    }

    public void MoveGhost(float x, float y)
    {
        serverGhost_.transform.localPosition = new Vector3(x, y, 0);
    }

    public void Snap(float x, float y)
    {
        desiredPosition_.x = x;
        desiredPosition_.y = y;
    }

    public void ApplyInputs(float dt)
    {
        var speed = Stats.GetFinValue(proto_game.Stats.MovementSpeed);
        Vector2 totalImpulse = new Vector2(speed, speed);
        totalImpulse.Scale(Force);

        if (totalImpulse != Vector2.zero)
        {
            Velocity = totalImpulse;
        }

        if (appliedRecoil_ > 0.001f)
        {
            appliedRecoil_ *= -1;
            var dir = AttackDirection; 
            dir.x *= appliedRecoil_;
            dir.y *= appliedRecoil_;
            RecoilVelocity = dir;
            appliedRecoil_ = 0.0f;
        }

        var friction = Mathf.Clamp01(1.0f - dt * Body.drag);
        RecoilVelocity *= friction;
        Velocity *= friction;

        if (Mathf.Approximately(RecoilVelocity.magnitude, 0.0f))
        {
            RecoilVelocity = Vector2.zero;
        }

        if (Mathf.Approximately(Velocity.magnitude, 0.0f))
        {
            Velocity = Vector2.zero;
        }

        totalImpulse = Velocity + RecoilVelocity;
        totalImpulse.x *= dt;
        totalImpulse.y *= dt;

        if (!Replaying)
        {
            previousPos_ = desiredPosition_;
        }
        desiredPosition_ += totalImpulse;
        AttackPosition = desiredPosition_;
    }

    public override void ApplyRecoil(float recoil)
    {
        appliedRecoil_ += recoil;
    }

    public void Smooth()
    {
        tightness_ = SmoothTightness;
        Smoothed = true;
    }

    public void SetPosition(float x, float y)
    {
        Position = new Vector2(x, y);
        desiredPosition_ = new Vector2(x, y);
    }

    public PhysicalState GetState()
    {
        PhysicalState state = new PhysicalState();
        state.Position = desiredPosition_;
        state.Rotation = Rotation;
        state.Velocity = Velocity;
        state.RecoilVelocity = RecoilVelocity;
        state.Recoil = appliedRecoil_;
        return state;
    }

    public override void OnRemove()
    {
        base.OnRemove();
        if (weapon_ != null)
        {
            weapon_.gameObject.ReturnPooled();
            weapon_ = null;
        }
        if (nicknameText_ != null)
        {
            nicknameText_.gameObject.ReturnPooled();
            nicknameText_ = null;
        }
    }

    public void AddStatus(proto_game.PowerUpType effectType, float duration)
    {
        Status statusEffect = new Status(effectType, duration);
        statusManager_.Add(statusEffect);
    }
}
