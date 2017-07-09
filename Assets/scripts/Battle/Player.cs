using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using Nito;
using arena.common.battle;

public class Player : Unit, PlayerExperience.IExpProvider 
{
    private static readonly float SmoothTightness = 0.05f;
    private static readonly float DefaulTightness = 0.1f;

    [SerializeField]
    private Vector3 nicknameOffset = Vector3.zero;

    [SerializeField]
    private proto_profile.PlayerClasses plrClass = proto_profile.PlayerClasses.Assault;

    private Vector2 desiredPosition_ = Vector2.zero;
    private PlayerExperience playerExp_;
    private Text nicknameText_ = null;
    private StatusManager statusManager_;
    private Vector2 previousPos_ = Vector2.zero;
    private float posAlpha_ = 0;
    private float tightness_ = 0;
    private float appliedRecoil_ = 0;
    private Vector2 lastServerPosition_ = Vector2.zero;
    private bool firstPositionSet_ = true;
    private Dictionary<int, List<Bullet>> bulletsHistory_ = new Dictionary<int, List<Bullet>>();
    private List<Bullet> bulletsFired_ = null;

    private GameObject serverGhost_ = null;

	public Player()
	{
        playerExp_ = new PlayerExperience(this);
        statusManager_ = new StatusManager(this);
        Input = new proto_game.PlayerInput.Request();
        IsNetworked = true;
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

    public Vector2 LastServerPosition
    {
        get { return lastServerPosition_; }
    }

    public PlayerExperience PlayerExperience
    {
        get { return playerExp_; }
    }

    private Vector2 force_;
    public Vector2 Force
    {
        set 
        {   
            force_ = value;
        }
        get { return force_; }
    }

    public override void Init(arena.ArenaController controller)
    {
        base.Init(controller);
        firstPositionSet_ = true;
        Level = 1;
        Replaying = false;
        DisableVelocityMovement = true;
    }

    protected override void OnPositionSet()
    {
        base.OnPositionSet();

        if (firstPositionSet_)
        {
            firstPositionSet_ = false;
            desiredPosition_ = Position;
        }
    }

    public override void OnUpdate(float dt)
    {
        statusManager_.Update(dt);

        if (Local)
        {
            posAlpha_ = InterpolationBaseValue / GameApp.Instance.MovementUpdateDT;

            Position = Position + (desiredPosition_ - Position) * tightness_;
            tightness_ += (DefaulTightness - tightness_) * 0.01666f;

            if (!Smoothed)
            {
                Position = Vector2.Lerp(previousPos_, desiredPosition_, posAlpha_);
            }
            else
            {
                Position = Position + (desiredPosition_ - Position) * tightness_;
                tightness_ += (DefaulTightness - tightness_) * 0.01666f;

                if ((desiredPosition_-Position).magnitude < 0.3f)
                {
                    Smoothed = false;
                }
            }
        }

        base.OnUpdate(dt);

        if (nicknameText_ != null)
        {
            nicknameText_.transform.position = transform.localPosition + nicknameOffset;
        }

        foreach(var bullets in bulletsHistory_)
        {
            foreach(var b in bullets.Value)
            {
                b.OnUpdate(dt);
            }
        }
    }

    public void CreateServerGhost()
    {
        serverGhost_ = gameObject.GetPooled();
        serverGhost_.GetComponent<Player>().Collider.enabled = false;
        Controller.gameObject.AddChild(serverGhost_);
    }

    public void SetLastServerPosition(float x, float y)
    {
        lastServerPosition_.x = x;
        lastServerPosition_.y = y;
    }

    public void MoveGhost(float x, float y)
    {
        if (serverGhost_ != null)
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

        //if (totalImpulse != Vector2.zero)
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
        //Velocity *= friction;

        if (Mathf.Abs(RecoilVelocity.magnitude) <= 0.001f)
        {
            RecoilVelocity = Vector2.zero;
        }

        if (Mathf.Abs(Velocity.magnitude) <= 0.001f)
        {
            Velocity = Vector2.zero;
        }

        totalImpulse = RecoilVelocity + Velocity;
        totalImpulse.x *= dt;
        totalImpulse.y *= dt;

        if (!Replaying)
        {
            previousPos_ = desiredPosition_;
        }

        desiredPosition_ += totalImpulse;
        //Debug.LogWarningFormat("totalImpulse {0} {1}",totalImpulse.x,totalImpulse.y);
        AttackPosition = desiredPosition_;
    }

    public override void ApplyRecoil(float recoil)
    {
        //appliedRecoil_ += recoil;
    }

    public void Smooth()
    {
        tightness_ = SmoothTightness;
        Smoothed = true;
    }

    public void SetDesiredPosition(float x, float y)
    {
        Position = new Vector2(x, y);
        desiredPosition_ = new Vector2(x, y);
    }

    public PhysicalState GetState()
    {
        PhysicalState state = new PhysicalState();
        state.Position = desiredPosition_;
        state.Rotation = Rotation;
        state.RecoilVelocity = RecoilVelocity;
        state.Recoil = appliedRecoil_;
        return state;
    }

    protected override void OnRemove()
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

        if (serverGhost_ != null)
        {
            serverGhost_.ReturnPooled();
            serverGhost_ = null;
        }

        foreach(var p in bulletsHistory_)
        {
            ListPool<Bullet>.Release(p.Value);
        }

        bulletsHistory_.Clear();
    }

    public void AddStatus(proto_game.PowerUpType effectType, float duration)
    {
        Status statusEffect = new Status(effectType, duration);
        statusManager_.Add(statusEffect);
    }

    public void PreInputActions(int inputID)
    {
        bulletsFired_ = ListPool<Bullet>.Get();
        bulletsHistory_.Add(inputID, bulletsFired_);
    }

    public void PostInputActions(int inputID)
    {
        if (bulletsFired_.Count == 0)
        {
            ListPool<Bullet>.Release(bulletsHistory_[inputID]);
            bulletsHistory_.Remove(inputID);
        }
    }

    public void OnBulletShoot(Bullet bullet)
    {
        //Debug.LogFormat("OnBulletShoot {0} {1}", bullet.ID, bullet.TickOfCreation);
        bulletsFired_.Add(bullet);
        bullet.ID = -1;
    }

    public void SyncBulletsWithServer(int inputID, int firstBulletId)
    {
        List<Bullet> bullets = null;
        //Debug.LogFormat("SyncBulletsWithServer inputID {0}", inputID);
        bulletsHistory_.TryGetValue(inputID, out bullets);
        if (bullets != null)
        {
            bulletsHistory_.Remove(inputID);
            foreach(var blt in bullets)
            {
                blt.ID = firstBulletId++;
                if (blt.TriggerDamageOnSync)
                {
                    //damage was delayed
                    foreach(var target in blt.Targets)
                        Controller.OnBulletCollision(target, blt);
                    blt.OnExpired();
                    //Debug.LogFormat("SyncBulletsWithServer {0} {1}", blt.ID, blt.TickOfCreation);
                }
                else 
                {
                    controller_.Add(blt);
                }
            }
            ListPool<Bullet>.Release(bullets);
        }
    }

    public bool IsSyncedWithServer(Bullet bullet)
    {
        return !bulletsHistory_.ContainsKey(bullet.TickOfCreation);
    }

    public void BulletExpired(Bullet bullet)
    {
        if (!IsSyncedWithServer(bullet)) 
            return;

        Controller.OnBulletExpired(bullet);
        UnregisterBullet(bullet);

        List<Bullet> bullets = null;
        bulletsHistory_.TryGetValue(bullet.TickOfCreation, out bullets);
        if (bullets != null)
        {
            bullets.Remove(bullet);
            if (bullets.Count == 0)
            {
                bulletsHistory_.Remove(bullet.TickOfCreation);
                //Debug.LogFormat("BulletExpired {0} {1}", bullet.ID, bullet.TickOfCreation);
                ListPool<Bullet>.Release(bullets);
            }
        }
    }

    public void RemoveBulletsLinkedWithTarget(Entity target)
    {
        /*
        foreach(var pair in bulletsHistory_)
        {
            var bullets = pair.Value;
            var length = bullets.Count;
            for(int i = 0; i < length; ++i)
            {
                int total = bullets[i].Targets.Count;
                if (bullets[i].Targets.Count == 1 && bullets[i].Targets[i] == target)
                {
                    bullets[i].TriggerDamageOnSync = false;
                }
            }
        }
        */
    }
}
