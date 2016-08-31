using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace arena
{
    public class ArenaController : Scene 
    {
        private static readonly float SyncGameTickPeriod = 2.0f;

        [Header("Arena Set-Up")]
        [SerializeField]
        private Joystick moveJoystick_ = null;

        [SerializeField]
        private Joystick shootJoystick_ = null;

        [SerializeField]
        private GameObject bulletPrefab = null;

        [SerializeField]
        private GameObject hpBarsPrefab = null;

        [SerializeField]
        private GameObject nicknamePrefab = null;

        [SerializeField]
        private Canvas worldUIContainer = null;

        [SerializeField]
        private SmoothFollow followCamera = null;

        [SerializeField]
        private ExpBlocksDict blockPrefabs = null;

        [SerializeField]
        private PlayerClassesDict playerClassesPrefabs = null;

        [SerializeField]
        private PowerUpsDict powerUpPrefabs = null;

        [SerializeField]
        private ui.ArenaUIController arenaUI = null;

        [SerializeField]
        private GameObject background_ = null;

        [SerializeField]
        private EdgeCollider2D[] walls = null;

        private WaitForSeconds waitForSync_ = new WaitForSeconds(SyncGameTickPeriod);
        private Dictionary<int, ArenaObject> entities_ = new Dictionary<int, ArenaObject>();
        private List<PowerUp> powerUps_ = new List<PowerUp>();
        private List<Player> players_ = new List<Player>();
        private InputHistory inputHistory_ = new InputHistory();
        private List<proto_game.PlayerInput.Response> syncInputsQueue_ = new List<proto_game.PlayerInput.Response>();
        private Player player_ = null;
        private float accumulator_ = 0.0f;
        private float gameTime_ = 0.0f;
        private bool castSkill_;
        private NetworkProjectileManager networkProjectileManager_ = new NetworkProjectileManager();
        private List<ArenaObject> removeList_ = new List<ArenaObject>();

        public int Tick
        { get; private set; }

        public int FixedTick
        { get; private set; }

        public float GameTime
        {
            get { return gameTime_; }
        }

        public float TickToFloatTime(int tick)
        {
            return (float)tick * GameApp.Instance.MovementUpdateDT;
        }

        public proto_game.PlayerInput.Request PlayerInput
        { get; set; }

        public override void OnBeforeShow()
        {
            GameApp.Instance.Client.OnServerEvent += HandleEvent;
            GameApp.Instance.Client.OnServerResponse += HandleResponse;

            Ready = false;
            arenaUI.StatsPanel.OnStatUpgrade = HandleUpgradeStat;
            networkProjectileManager_.Clear();

            var findRequest = new proto_game.FindRoom.Request();
            var id = GameApp.Instance.Client.Send(findRequest, proto_common.Commands.FIND_ROOM);

            GameApp.Instance.RequestsManager.AddRequest(id, 
            (proto_common.Response response)=>
            {
                if (response.error == (int)proto_common.Common.CommonErrors.CE_NO_ERROR)
                {
                    OnJoinGame();
                }
            });
        }

        public override void OnAfterShow()
        {
            StartCoroutine(SyncGameTicks());
        }

        private IEnumerator SyncGameTicks()
        {
            while (true)
            {
                yield return waitForSync_;

                var syncRequest = new proto_game.SyncTick.Request();
                var id = GameApp.Instance.Client.Send(syncRequest, proto_common.Commands.SYNC_TICK);
                GameApp.Instance.RequestsManager.AddRequest(id, (proto_common.Response response)=>
                {
                    var syncResponse = response.Extract<proto_game.SyncTick.Response>(proto_common.Commands.SYNC_TICK);
                    AdjustGameTime(syncResponse.tick);
                });
            }
        }

        public override void OnBeforeHide()
        {
            DG.Tweening.DOTween.KillAll();
            ResetState();

            GameApp.Instance.Client.OnServerEvent -= HandleEvent;
            GameApp.Instance.Client.OnServerResponse -= HandleResponse;
        }

        private void ResetState()
        {
            foreach(var entity in entities_)
            {
                Destroy(entity.Value, false);
            }
            ProcessRemoveList();
            player_ = null;
        }

        private void OnGameJoin()
        {
            FixedTick = 0;
            accumulator_ = 0;
            inputHistory_.Reset();
            gameTime_ = 0;
        }

        private Player CreatePlayer(proto_profile.PlayerClasses cl)
        {
            var player = playerClassesPrefabs[cl].GetPooled();
            //turn a bit to left our sprites
            var playerScript = player.GetComponent<Player>();
            playerScript.Rotation = 90;
            playerScript.PlayerExperience.OnLevelUp = HandleLevelUp;
            playerScript.Local = false;

            var nameObj = nicknamePrefab.GetPooled();
            playerScript.NicknameText = nameObj.GetComponent<Text>();

            worldUIContainer.gameObject.AddChild(nameObj);

            return playerScript;
        }

        public void ReturnHpBar(GameObject bar)
        {
            bar.ReturnPooled();
        }

    	private void Update () 
        {
            if (player_ == null) 
                return;

            //just to make rotation smooth
            if (shootJoystick_.inputDirection != Vector2.zero)
            {
                player_.SetRotation(shootJoystick_.inputDirection);
            }
    	}

        private void FixedUpdate()
        {
            FixedTick++;
        }

        private void LateUpdate()
        {
            if (player_ == null)
                return;

            accumulator_ += Time.deltaTime;
            float inputDt = GameApp.Instance.MovementUpdateDT;

            while (accumulator_ >= inputDt)
            {
                accumulator_ -= inputDt;
                SendLocalPlayerInput();
            }

            player_.InterpolationBaseValue = accumulator_;

            foreach(var sync in syncInputsQueue_)
            {
                inputHistory_.Correction(sync, player_);
                player_.SetLastServerPosition(sync.x, sync.y);
            }
            syncInputsQueue_.Clear();

            foreach(var p in entities_)
            {
                p.Value.OnUpdate(Time.deltaTime);
            }

            gameTime_ += Time.deltaTime;
            networkProjectileManager_.CorrectProjectilesRelativelyToLocalPlayer();

            ProcessRemoveList();
        }

        private void ProcessRemoveList()
        {
            foreach(var obj in removeList_)
            {
                entities_.Remove(obj.ID);

                if (obj is Player)
                {
                    players_.Remove(obj as Player);
                }
            }
            removeList_.Clear();
        }

        public void SendLocalPlayerInput()
        {
            Vector3 dir = Vector3.zero;

            dir.x = Input.GetAxis("Horizontal");
            dir.y = Input.GetAxis("Vertical");

            dir = dir.normalized;

            if (moveJoystick_.inputDirection != Vector2.zero)
            {
                dir = moveJoystick_.inputDirection;
            }

            player_.Force = dir;

            var input = player_.Input;
            input.shoot = false;
            input.skill = false;

            input.tick = Tick;
            GetLocalPlayerMoves(input);
            player_.PreInputActions(Tick);
            if (shootJoystick_.inputDirection != Vector2.zero)
            {
                if (player_.CanAttack())
                {
                    input.shoot = true;
                    player_.PerformAttack(shootJoystick_.inputDirection);
                    player_.SetAttackCooldown();
                }
            }

            if (castSkill_)
            {
                castSkill_ = false;
                if (player_.CanCast())
                {
                    input.skill = true;
                }
            }

            input.rotation = player_.Rotation;
            inputHistory_.Add(input, player_.GetState());
            GameApp.Instance.Client.Send(input, proto_common.Commands.PLAYER_INPUT);

            player_.ApplyInputs(GameApp.Instance.MovementUpdateDT);

            player_.PostInputActions(Tick);
            Tick++;
        }

        public bool IsLocalPlayer(Unit unit)
        {
            return player_ != null && (unit as Player) == player_;
        }

        private void GetLocalPlayerMoves(proto_game.PlayerInput.Request moveReq)
        {
            moveReq.force_x = player_.Force.x;
            moveReq.force_y = player_.Force.y;
        }

        #region Entry network point
        private void HandleEvent(proto_common.Event evt) 
        {
            if (evt.type == proto_common.Events.UNIT_STATE_UPDATE) 
            {
                HandleUnitStateUpdate(evt);
            } 
            else if (evt.type == proto_common.Events.PLAYER_APPEARED)
            {
                HandlePlayerAppeared(evt);
            } 
            else if (evt.type == proto_common.Events.PLAYER_DISCONNECTED)
            {
                HandlePlayerDisconnected(evt);
            }
            else if (evt.type == proto_common.Events.PLAYER_TURN)
            {
                HandlePlayerTurned(evt);
            }
            else if (evt.type == proto_common.Events.UNIT_ATTACK)
            {
                HandleUnitAttack(evt);
            }
            else if (evt.type == proto_common.Events.DAMAGE_DONE)
            {
                HandleUnitDamage(evt);
            }
            else if (evt.type == proto_common.Events.PLAYER_STATUS_CHANGED)
            {
                HandlePlayerStatusChanged(evt);
            }
            else if (evt.type == proto_common.Events.UNIT_DIE)
            {
                HandleUnitDie(evt);
            }
            else if (evt.type == proto_common.Events.BLOCK_APPEARED)
            {
                HandleBlockAppeared(evt);
            }
            else if (evt.type == proto_common.Events.PLAYER_EXPERIENCE)
            {
                HandlePlayerExperience(evt);
            }
            else if (evt.type == proto_common.Events.GAME_FINISHED)
            {
                HandleGameFinished(evt);
            }
            else if (evt.type == proto_common.Events.POWER_UP_APPEARED)
            {
                HandlePowerUpAppeared(evt);
            }
            else if (evt.type == proto_common.Events.POWER_UP_GRABBED)
            {
                HandlePowerUpGrabbed(evt);
            }
            else if (evt.type == proto_common.Events.SKILL_CASTED)
            {
                HandleSkillCasted(evt);
            }
            else if (evt.type == proto_common.Events.MOB_APPEARED)
            {
                HandleMobAppeared(evt);
            }
            else if (evt.type == proto_common.Events.BULLET_DESTROYED)
            {
                HandleBulletDestroyed(evt);
            }
        }

        private void HandleResponse(proto_common.Response response)
        {
            if (response.type == proto_common.Commands.JOIN_GAME)
            {
                HandleJoinGame(response);
            }
            else if (response.type == proto_common.Commands.PLAYER_INPUT)
            {
                HandleLocalPlayerInputCorrection(response);
            }
        }
        #endregion

        #region Network Handlers
        private void HandleBulletDestroyed(proto_common.Event evt)
        {
            var bulletPacket = evt.Extract<proto_game.BulletsRemoved>(proto_common.Events.BULLET_DESTROYED);
            foreach(var id in bulletPacket.guid)
            {
                Destroy(id);
            }
        }

        private void HandleLocalPlayerInputCorrection(proto_common.Response response)
        {
            var syncResponse = response.Extract<proto_game.PlayerInput.Response>(proto_common.Commands.PLAYER_INPUT);
            syncInputsQueue_.Add(syncResponse);
            player_.MoveGhost(syncResponse.x, syncResponse.y);
        }

        private void HandleMobAppeared(proto_common.Event evt)
        {
            var mobEvt = evt.Extract<proto_game.MobAppeared>(proto_common.Events.MOB_APPEARED);

            var mobObj = MobPrefabs.Instance.Get(mobEvt.type);
            var mobScript = mobObj.GetComponent<Mob>();
            mobScript.WeaponUsed = mobEvt.weapon_used;
            mobScript.Init(this);
            mobScript.Position = new Vector2(mobEvt.x, mobEvt.y);
            mobScript.ID = mobEvt.id;
            mobScript.Rotation = 90;
            mobScript.AttackRange = mobEvt.attack_range;
            mobScript.IsNetworked = true;
            mobScript.ApplyStats(mobEvt.stats);

            CreateHpBar(mobScript);
            mobScript.Health = mobEvt.hp;
            Add(mobScript);
            //mobScript.CreateGhost();
        }

        private void HandleSkillCasted(proto_common.Event evt)
        {
            var skillCastedEvt = evt.Extract<proto_game.SkillCasted>(proto_common.Events.SKILL_CASTED);
            var player = GetObject(skillCastedEvt.guid) as Player;

            player.Rotation = skillCastedEvt.direction;
            player.AttackPosition = new Vector2(skillCastedEvt.x, skillCastedEvt.y);
            player.CastSkill();
        }

        private void HandlePowerUpAppeared(proto_common.Event evt)
        {
            var powerUpEvt = evt.Extract<proto_game.PowerUpAppeared>(proto_common.Events.POWER_UP_APPEARED);

            var powerUpObject = powerUpPrefabs[powerUpEvt.type].GetPooled();
            var powerUpScript = powerUpObject.GetComponent<PowerUp>();

            powerUps_.Add(powerUpScript);
            powerUpScript.ID = powerUpEvt.id;
            powerUpScript.RemoveAfter = (float)powerUpEvt.lifetime / 1000;

            powerUpObject.transform.position = new Vector3(powerUpEvt.x, powerUpEvt.y, 0);
            Add(powerUpScript);
        }

        private void HandlePowerUpGrabbed(proto_common.Event evt)
        {
            var powerUpEvt = evt.Extract<proto_game.PowerUpGrabbed>(proto_common.Events.POWER_UP_GRABBED);
            var whoGrabbed = GetObject(powerUpEvt.who_grabbed) as Player;
            var grabbedWhat = GetObject(powerUpEvt.id) as PowerUp;

            if (whoGrabbed != null && grabbedWhat!=null)
            {
                whoGrabbed.AddStatus(grabbedWhat.PowerUpType, grabbedWhat.RemoveAfter);
                Remove(grabbedWhat);
            }
        }

        private void HandleGameFinished(proto_common.Event evt)
        {
            var finishedResponse = evt.Extract<proto_game.GameFinished>(proto_common.Events.GAME_FINISHED);
            User.Instance.Coins += finishedResponse.coins;
            User.Instance.ProfileExperience.AddExperience(finishedResponse.exp);
            ShowHeroSelection();
        }

        private void ShowHeroSelection()
        {
            SceneManager.Instance.SetActive(SceneManager.Scenes.SelectHero);
        }

        private void HandleJoinGame(proto_common.Response response)
        {
            var joinResponse = response.Extract<proto_game.JoinGame.Response>(proto_common.Commands.JOIN_GAME);

            //onGameJoin will create player script
            OnGameJoin();
            arenaUI.Init(joinResponse.time_left, player_);

            var bottomLeft = new Vector2(joinResponse.outer_border[0], joinResponse.outer_border[1]);
            var topRight = new Vector2(joinResponse.outer_border[2], joinResponse.outer_border[3]);
            background_.transform.localScale = new Vector3(topRight.x-bottomLeft.x, topRight.y-bottomLeft.y, 1);
            Tick = joinResponse.tick;
        }

        private void HandlePlayerExperience(proto_common.Event evt)
        {
            var expPacket = evt.Extract<proto_game.PlayerExperience>(proto_common.Events.PLAYER_EXPERIENCE);

            player_.PlayerExperience.AddExperience(expPacket.exp);
            arenaUI.DrawLevelData(player_.PlayerExperience);
        }

        private void HandlePlayerDisconnected(proto_common.Event evt)
        {
            proto_game.PlayerDisconnected disconnectPacket = 
                ProtoBuf.Extensible.GetValue<proto_game.PlayerDisconnected> (evt, (int)proto_common.Events.PLAYER_DISCONNECTED);

            Destroy(disconnectPacket.who);
        }

        private void HandleBlockAppeared(proto_common.Event evt)
        {
            var unitPacket = evt.Extract<proto_game.BlockAppeared>(proto_common.Events.BLOCK_APPEARED);

            GameObject blockPrefab = blockPrefabs[unitPacket.type];
            GameObject blockObject = blockPrefab.GetPooled();
            ExpBlock block = blockObject.GetComponent<ExpBlock>();
            block.Init(this);
            block.ID = unitPacket.guid;
            block.Position = new Vector2(unitPacket.position.x, unitPacket.position.y);
            CreateHpBar(block);

            block.Stats.SetValue(proto_game.Stats.MaxHealth, unitPacket.max_hp);
            block.Health = unitPacket.hp;

            Add(block);
        }

        public void AddToScene(ArenaObject entity)
        {
            gameObject.AddChild(entity.gameObject);
        }

        public void Add(ArenaObject entity)
        {
            AddToScene(entity);

            if (entities_.ContainsKey(entity.ID))
            {
                Debug.LogFormat("{0} {1}", entity.ID, entity.GetType().ToString());
            }
            else
            {
                entities_.Add(entity.ID, entity);
            }

            if (entity is Bullet)
            {
                networkProjectileManager_.Register(entity as Bullet);
            }
        }

        private void HandleUnitStateUpdate(proto_common.Event evt)
        {
            proto_game.UnitStatesUpdate updatesPacket = 
                ProtoBuf.Extensible.GetValue<proto_game.UnitStatesUpdate> (evt, (int)proto_common.Events.UNIT_STATE_UPDATE);

            var length = updatesPacket.states.Count;
            var states = updatesPacket.states;
            AdjustGameTime(updatesPacket.tick);

            for (var i = 0; i < length; ++i)
            {
                var stateData = states[i];
                var unit = GetObject(stateData.guid) as Unit;

                if (unit != null)
                {
                    unit.UpdateState(stateData, updatesPacket.tick);
                }
            }
        }

        private ArenaObject GetObject(int id)
        {
            ArenaObject obj = null;
            entities_.TryGetValue(id, out obj);
            Debug.AssertFormat(obj != null, "Unknown object with id {0}", id);
            return obj;
        }

        private void HandlePlayerAppeared(proto_common.Event evt)
        {
            proto_game.PlayerAppeared plrPacket = 
                ProtoBuf.Extensible.GetValue<proto_game.PlayerAppeared> (evt, (int)proto_common.Events.PLAYER_APPEARED);

            Player playerToInited = null;

            if (!plrPacket.local)
            {
                playerToInited = CreatePlayer(plrPacket.@class);
            }
            else 
            {
                player_ = CreatePlayer(plrPacket.@class);
                playerToInited = player_;
                player_.Level = plrPacket.level;
                followCamera.SetTarget(playerToInited.gameObject);
                followCamera.SnapNextTick();

                if (player_.Level > 1)
                {
                    arenaUI.ShowUpgradePanel(player_.Level - 1);
                }
                networkProjectileManager_.Player = player_; 
                //local player sent, start game
                Ready = true;
            }

            playerToInited.ID = plrPacket.guid;
            //first apply stats, some methods use them inside
            playerToInited.ApplyStats(plrPacket.stats);
            playerToInited.Init(this);
            playerToInited.SetSkill(plrPacket.skill);
            playerToInited.Position = new Vector2(plrPacket.position.x, plrPacket.position.y);
            playerToInited.Nickname = plrPacket.name;

            CreateHpBar(playerToInited);

            playerToInited.Health = plrPacket.hp;
           
            Add(playerToInited);

            players_.Add(playerToInited);

            if (playerToInited == player_)
            {
                player_.Local = true;
                player_.CreateServerGhost();
            }
        }

        private void CreateHpBar(Entity entity)
        {
            var hpBar = hpBarsPrefab.GetPooled().GetComponent<AnimatedProgress>();
            hpBar.SetProgreessNotAnimated(1.0f);
            hpBar.Hide();
            entity.HpBar = hpBar;
            worldUIContainer.gameObject.AddChild(hpBar.gameObject);
        }

        private void HandlePlayerTurned(proto_common.Event evt)
        {
            proto_game.PlayerTurn turnPacket = evt.Extract<proto_game.PlayerTurn>(proto_common.Events.PLAYER_TURN);

            var unit = GetObject(turnPacket.guid) as Player;

            if (unit != null)
                unit.Rotation = turnPacket.angle;
        }

        private void HandleUnitAttack(proto_common.Event evt)
        {
            proto_game.UnitAttack attPacket = evt.Extract<proto_game.UnitAttack>(proto_common.Events.UNIT_ATTACK);
            var unit = GetObject(attPacket.guid) as Unit;
            if (unit != null)
            {
                if (attPacket.local)
                {
                    player_.SyncBulletsWithServer(attPacket.tick, attPacket.first_bullet_id);
                }
                else 
                {
                    unit.AttackPosition = new Vector2(attPacket.x, attPacket.y);
                    unit.PerformAttack(attPacket.direction*Mathf.Rad2Deg, 
                        TickToLongTime(attPacket.tick)+attPacket.time_advance, 
                        attPacket.first_bullet_id);
                }
            }
        }

        private long TickToLongTime(int tick)
        {
            return tick * (long)(GameApp.Instance.MovementUpdateDT*1000);
        }

        private void HandlePlayerStatusChanged(proto_common.Event evt)
        {
            proto_game.PlayerStatusChange statusEvt = evt.Extract<proto_game.PlayerStatusChange>(proto_common.Events.PLAYER_STATUS_CHANGED);

            var player = GetObject(statusEvt.guid) as Player;
            if (player != null)
            {
                player.Health = statusEvt.hp;
            }
        }

        private void HandleUnitDamage(proto_common.Event evt)
        {
            var damageEvt = evt.Extract<proto_game.DamageDone>(proto_common.Events.DAMAGE_DONE);

            var target = GetObject(damageEvt.target) as Unit;
            if (target != null)
            {
                target.Health = damageEvt.hp_left;
            }
        }

        private void HandleUnitDie(proto_common.Event evt)
        {
            proto_game.UnitDie unitDiePacket = evt.Extract<proto_game.UnitDie>(proto_common.Events.UNIT_DIE);
            var entity = GetObject(unitDiePacket.guid) as Entity;
            if (entity != null)
            {
                if (entity == player_)
                {
                    ShowHeroSelection();
                }
                Destroy(entity);
                player_.RemoveBulletsLinkedWithTarget(entity);
            }
        }

        #endregion

        public void HandleCastSkill()
        {
            castSkill_ = true;
        }

        public void OnBulletCollision(Entity target, Bullet bullet)
        {
            var damageRequest = new proto_game.DamageApply.Request();
            damageRequest.attacker = bullet.Owner.ID;
            damageRequest.bullet = bullet.ID;
            damageRequest.damage = bullet.Damage;
            damageRequest.target = target.ID;

            GameApp.Instance.Client.Send(damageRequest, proto_common.Commands.DAMAGE_APPLY);

            target.Health -= bullet.Damage;
        }

        private void HandleUpgradeStat(proto_game.Stats stat)
        {
            proto_game.StatUpgrade.Request statReq = new proto_game.StatUpgrade.Request();
            statReq.stat = stat;

            int id = GameApp.Instance.Client.Send(statReq, proto_common.Commands.STAT_UPGRADE);
            GameApp.Instance.RequestsManager.AddRequest(id, (proto_common.Response response)=>
            {
                if (response.error == 0)
                {
                    player_.Stats.Get(stat).IncreaseByStep();
                }
            });
        }

        private void HandleLevelUp(int level)
        {
            arenaUI.ShowUpgradePanel(1);
        }

        public void OnBulletExpired(Bullet bullet)
        {
            Destroy(bullet);
        }

        private void Destroy(int who)
        {
            var entity = GetObject(who);
            if (entity != null)
            {
                Destroy(entity);
            }
        }

        private void Destroy(ArenaObject entity, bool animated = true)
        {
            entity.Remove(animated);
            Remove(entity);

            if (entity is Bullet)
            {
                networkProjectileManager_.Unregister(entity as Bullet);
            }
        }

        public void Remove(ArenaObject entity)
        {
            removeList_.Add(entity);
        }

        private void AdjustGameTime(int tick)
        {
            var newGameTime = TickToFloatTime(tick);
            if (tick - Tick >= 2 || tick < Tick)
            {
                gameTime_ = newGameTime;
                Tick = tick;
            }
        }

        public void OnJoinGame()
        {
            var joinReq = new proto_game.JoinGame.Request();
            joinReq.@class = User.Instance.ClassSelected;
            GameApp.Instance.Client.Send(joinReq, proto_common.Commands.JOIN_GAME);
        }
    }
}