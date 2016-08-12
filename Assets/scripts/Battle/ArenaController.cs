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
        private Dictionary<int, Entity> entities_ = new Dictionary<int, Entity>();
        private List<PowerUp> powerUps_ = new List<PowerUp>();
        private List<Player> players_ = new List<Player>();
        private Dictionary<Bullet, Bullet> bullets_ = new Dictionary<Bullet, Bullet>();
        private InputHistory inputHistory_ = new InputHistory();
        private List<proto_game.PlayerInput.Response> syncInputsQueue_ = new List<proto_game.PlayerInput.Response>();
        private Player player_ = null;
        private float accumulator_ = 0.0f;
        private bool castSkill_;

        public int Tick
        { get; private set; }

        public int FixedTick
        { get; private set; }

        public proto_game.PlayerInput.Request PlayerInput
        { get; set; }

        public override void OnBeforeShow()
        {
            GameApp.Instance.Client.OnServerEvent += HandleEvent;
            GameApp.Instance.Client.OnServerResponse += HandleResponse;

            Ready = false;
            arenaUI.StatsPanel.OnStatUpgrade = HandleUpgradeStat;

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
                    if (syncResponse.tick > Tick)
                        Tick = syncResponse.tick + 1;
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
                DestroyEntity(entity.Value, false);
            }

            foreach(var bullet in bullets_)
            {
                bullet.Value.RemoveFromBattle(false);
            }
           
            player_ = null;
            bullets_.Clear();
            entities_.Clear();
            players_.Clear();
        }

        private void OnGameJoin()
        {
            FixedTick = 0;
            accumulator_ = 0;
            inputHistory_.Reset();
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

        public void AddBullet(Bullet bullet)
        {
            gameObject.AddChild(bullet.gameObject);
            bullets_.Add(bullet, bullet);
        }

        public Bullet CreateBullet()
        {
            GameObject bullet = bulletPrefab.GetPooled();
            if (bullet == null) return null;
            return bullet.GetComponent<Bullet>();
        }

        public void ReturnBullet(Bullet bullet)
        {
            bullets_.Remove(bullet);
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
                player_.MoveGhost(sync.x, sync.y);
            }
            syncInputsQueue_.Clear();

            foreach(var p in entities_)
            {
                p.Value.OnUpdate(Time.deltaTime);
            }
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

            if (shootJoystick_.inputDirection != Vector2.zero)
            {
                if (player_.PerformAttack(shootJoystick_.inputDirection))
                {
                    input.shoot = true;
                }
            }

            if (castSkill_)
            {
                castSkill_ = false;
                if (player_.CanCast())
                {
                    player_.CastSkill();
                    input.skill = true;
                }
            }

            input.rotation = player_.Rotation;
            inputHistory_.Add(input, player_.GetState());

            GameApp.Instance.Client.Send(input, proto_common.Commands.PLAYER_INPUT);
            Tick++;

            player_.ApplyInputs(GameApp.Instance.MovementUpdateDT);
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
        private void HandleLocalPlayerInputCorrection(proto_common.Response response)
        {
            var syncResponse = response.Extract<proto_game.PlayerInput.Response>(proto_common.Commands.PLAYER_INPUT);
            syncInputsQueue_.Add(syncResponse);
        }

        private void HandleMobAppeared(proto_common.Event evt)
        {
            var mobEvt = evt.Extract<proto_game.MobAppeared>(proto_common.Events.MOB_APPEARED);

            var mobObj = MobPrefabs.Instance.Get(mobEvt.type);
            var entityScript = mobObj.GetComponent<Unit>();
            entityScript.WeaponUsed = mobEvt.weapon_used;
            entityScript.Init(this, new Vector2(mobEvt.x, mobEvt.y));
            entityScript.ID = mobEvt.id;
            entityScript.Rotation = 90;
            entityScript.AttackRange = mobEvt.attack_range;
            entityScript.Stats.SetValue(proto_game.Stats.MaxHealth, mobEvt.max_hp);

            CreateHpBar(entityScript);
            entityScript.Health = mobEvt.hp;

            AddEntity(entityScript);
        }

        private void HandleSkillCasted(proto_common.Event evt)
        {
            var skillCastedEvt = evt.Extract<proto_game.SkillCasted>(proto_common.Events.SKILL_CASTED);
            var player = GetEntity(skillCastedEvt.guid) as Player;

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
            AddEntity(powerUpScript);
        }


        private void HandlePowerUpGrabbed(proto_common.Event evt)
        {
            var powerUpEvt = evt.Extract<proto_game.PowerUpGrabbed>(proto_common.Events.POWER_UP_GRABBED);
            var whoGrabbed = GetEntity(powerUpEvt.who_grabbed) as Player;
            var grabbedWhat = GetEntity(powerUpEvt.id) as PowerUp;

            if (whoGrabbed != null && grabbedWhat!=null)
            {
                whoGrabbed.AddStatus(grabbedWhat.PowerUpType, grabbedWhat.RemoveAfter);
                RemoveEntity(grabbedWhat);
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

            DestroyEntity(disconnectPacket.who);
        }

        private void HandleBlockAppeared(proto_common.Event evt)
        {
            var unitPacket = evt.Extract<proto_game.BlockAppeared>(proto_common.Events.BLOCK_APPEARED);

            GameObject blockPrefab = blockPrefabs[unitPacket.type];
            GameObject blockObject = blockPrefab.GetPooled();
            Entity entity = blockObject.GetComponent<Entity>();
            entity.Init(this, new Vector2(unitPacket.position.x, unitPacket.position.y));
            entity.ID = unitPacket.guid;

            CreateHpBar(entity);

            entity.Stats.SetValue(proto_game.Stats.MaxHealth, unitPacket.max_hp);
            entity.Health = unitPacket.hp;

            AddEntity(entity);

            entity.OnUpdate(0.0f);
        }

        private void AddEntity(Entity entity)
        {
            gameObject.AddChild(entity.gameObject);

            if (entities_.ContainsKey(entity.ID))
            {
                Debug.LogFormat("{0} {1}", entity.ID, entity.GetType().ToString());
            }
            else
            {
                entities_.Add(entity.ID, entity);
            }
        }

        private void HandleUnitStateUpdate(proto_common.Event evt)
        {
            proto_game.UnitStatesUpdate updatesPacket = 
                ProtoBuf.Extensible.GetValue<proto_game.UnitStatesUpdate> (evt, (int)proto_common.Events.UNIT_STATE_UPDATE);

            var length = updatesPacket.states.Count;
            var states = updatesPacket.states;
            for (var i = 0; i < length; ++i)
            {
                var stateData = states[i];
                Entity unit = GetEntity(stateData.guid);

                if (unit != null)
                {
                    unit.UpdateState(stateData, updatesPacket.timestamp);
                }
            }
        }

        private Entity GetEntity(int id)
        {
            Entity unit = null;
            entities_.TryGetValue(id, out unit);
            if (id == player_.ID) unit = player_;
            Debug.AssertFormat(unit != null, "Unknown entity with id {0}", id);
            return unit;
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
                //local player sent, start game
                Ready = true;
            }

            playerToInited.ID = plrPacket.guid;
            //first apply stats, some methods use them inside
            playerToInited.ApplyStats(plrPacket.stats);
            playerToInited.Init(this, new Vector2(plrPacket.position.x, plrPacket.position.y));
            playerToInited.SetSkill(plrPacket.skill);
            playerToInited.Nickname = plrPacket.name;

            CreateHpBar(playerToInited);

            playerToInited.Health = plrPacket.hp;

            AddEntity(playerToInited);

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

            Entity unit = GetEntity(turnPacket.guid);

            if (unit != null)
                unit.Rotation = turnPacket.angle;
        }

        private void HandleUnitAttack(proto_common.Event evt)
        {
            proto_game.UnitAttack attPacket = evt.Extract<proto_game.UnitAttack>(proto_common.Events.UNIT_ATTACK);

            Entity unit = GetEntity(attPacket.guid);
            if (unit != null)
            {
                unit.AttackPosition = new Vector2(attPacket.x, attPacket.y);
                unit.PerformAttack(attPacket.direction*Mathf.Rad2Deg, attPacket.timestamp);
            }
        }

        private void HandlePlayerStatusChanged(proto_common.Event evt)
        {
            proto_game.PlayerStatusChange statusEvt = evt.Extract<proto_game.PlayerStatusChange>(proto_common.Events.PLAYER_STATUS_CHANGED);

            Entity player = GetEntity(statusEvt.guid);
            if (player != null)
            {
                player.Health = statusEvt.hp;
            }
        }

        private void HandleUnitDamage(proto_common.Event evt)
        {
            var damageEvt = evt.Extract<proto_game.DamageDone>(proto_common.Events.DAMAGE_DONE);

            Entity target = GetEntity(damageEvt.target);
            if (target != null)
            {
                target.Health = damageEvt.hp_left;
            }
        }

        private void HandleUnitDie(proto_common.Event evt)
        {
            proto_game.UnitDie unitDiePacket = evt.Extract<proto_game.UnitDie>(proto_common.Events.UNIT_DIE);
            Entity entity = GetEntity(unitDiePacket.guid);
            if (entity != null)
            {
                if (entity == player_)
                {
                    ShowHeroSelection();
                }
                DestroyEntity(entity);
            }
        }

        #endregion

        public void HandleCastSkill()
        {
            castSkill_ = true;
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

        private void DestroyEntity(int who)
        {
            Entity entity = GetEntity(who);
            if (entity != null)
            {
                DestroyEntity(entity);
            }
        }

        private void DestroyEntity(Entity entity, bool rm = true)
        {
            entity.Remove(rm);

            if (rm)
            {
                RemoveEntity(entity);
            }
        }

        private void RemoveEntity(Entity entity)
        {
            entities_.Remove(entity.ID);

            if (entity is Player)
            {
                players_.Remove(entity as Player);
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