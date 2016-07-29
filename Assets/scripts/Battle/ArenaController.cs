using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System.Timers;

namespace arena
{
    public class ArenaController : Scene 
    {
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

        private Player player_ = null;
        private long prevTime_ = 0;
        private Vector2 prevPos;
        private Timer positionTimer_;
        private Dictionary<int, Entity> entities_ = new Dictionary<int, Entity>();
        private List<PowerUp> powerUps_ = new List<PowerUp>();
        private List<Player> players_ = new List<Player>();

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

        public override void OnBeforeHide()
        {
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

            if (player_ != null)
                player_.OnAttack -= HandlePlayerAttack;

            player_ = null;
            entities_.Clear();
            players_.Clear();

            if (positionTimer_ != null)
            {
                positionTimer_.Stop();
                positionTimer_.Dispose();
                positionTimer_ = null;
            }
        }

        private void OnGameJoin()
        {
            positionTimer_ = new Timer(1000 / GameApp.Instance.MovementUpdateFrequency);
            positionTimer_.Elapsed += (object sender, ElapsedEventArgs e) => 
            {
                Worker.Instance.Add(NotifyServerAboutPlayerPosition);
            };
            positionTimer_.Start();

            player_ = CreatePlayer(User.Instance.ClassSelected);

            Ready = true;
        }

        private Player CreatePlayer(proto_profile.PlayerClasses cl)
        {
            var player = playerClassesPrefabs[cl].GetPooled();
            //turn a bit to left our sprites
            player.transform.rotation = Quaternion.AngleAxis(90.0f, new Vector3(0,0,1));
            var playerScript = player.GetComponent<Player>();
            playerScript.PlayerExperience.OnLevelUp = HandleLevelUp;
            playerScript.Local = false;

            var nameObj = nicknamePrefab.GetPooled();
            playerScript.NicknameText = nameObj.GetComponent<Text>();

            worldUIContainer.gameObject.AddChild(nameObj);

            return playerScript;
        }

        public Bullet CreateBullet()
        {
            GameObject bullet = bulletPrefab.GetPooled();
            if (bullet == null) return null;
            gameObject.AddChild(bullet);
            return bullet.GetComponent<Bullet>();
        }

        public void ReturnBullet(Bullet bullet)
        {
            bullet.gameObject.ReturnPooled();
        }

        public void ReturnHpBar(GameObject bar)
        {
            bar.ReturnPooled();
        }

    	private void Update () 
        {
            if (player_ == null || !player_.Local) return;

    	    Vector3 dir = Vector3.zero;

            dir.x = Input.GetAxis("Horizontal");
            dir.y = Input.GetAxis("Vertical");

            dir = dir.normalized;

            if (moveJoystick_.inputDirection != Vector2.zero)
            {
                dir = moveJoystick_.inputDirection;
            }

            player_.Force = dir;

            if (shootJoystick_.inputDirection != Vector2.zero)
            {
                player_.PerformAttack(shootJoystick_.inputDirection);
            }

            foreach(var p in players_)
            {
                p.OnUpdate();
            }
    	}

        private void FixedUpdate()
        {
            foreach(var p in players_)
            {
                p.OnFixedUpdate();
            }
        }

        private bool NotifyServerAboutPlayerPosition()
        {
            if (player_ != null)
            {
                if (player_.Moved) 
                {
                    SendLocalPlayerMoved();
                }

                if (player_.Rotated)
                {
                    proto_game.PlayerTurn turnReq = new proto_game.PlayerTurn();
                    turnReq.angle = player_.Rotation;
                    turnReq.guid = player_.ID;

                    GameApp.Instance.Client.Send (turnReq, proto_common.Commands.TURN);
                }
            }
            return true;
        }

        private void SendLocalPlayerMoved(bool stop = false)
        {
            proto_game.PlayerMove.Request moveReq = new proto_game.PlayerMove.Request();

            Vector2 plrPosition = player_.Position;
            moveReq.position = new proto_game.Vector();
            moveReq.position.x = plrPosition.x;
            moveReq.position.y = plrPosition.y;
            moveReq.timestamp = GameApp.Instance.TimeMs();
            moveReq.stop = stop;

            prevPos = plrPosition;
            prevTime_ = moveReq.timestamp;

            GameApp.Instance.Client.Send (moveReq, proto_common.Commands.PLAYER_MOVE);
        }

        public void SendDamageDone(Entity target, float damage)
        {
            var request = new proto_game.ApplyDamage();
            request.damage = damage;
            request.target = target.ID;

            GameApp.Instance.Client.Send(request, proto_common.Commands.DAMAGE);
        }

        #region Entry network point
        private void HandleEvent(proto_common.Event evt) 
        {
            if (evt.type == proto_common.Events.UNIT_MOVE) 
            {
                HandleUnitMove(evt);
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
        }

        private void HandleResponse(proto_common.Response response)
        {
            if (response.type == proto_common.Commands.JOIN_GAME)
            {
                HandleJoinGame(response);
            }
        }
        #endregion

        public void OnPowerUpGrabbed(Player player, PowerUp powerUp)
        {
            var request = new proto_game.GrabPowerUp.Request();
            request.powerUp = powerUp.ID;

            var reqID = GameApp.Instance.Client.Send(request, proto_common.Commands.GRAB_POWERUP);
            GameApp.Instance.RequestsManager.AddRequest(reqID, (proto_common.Response response)=>
            {
                if (response.error == (int)proto_common.Common.CommonErrors.CE_NO_ERROR)
                {
                    player.AddStatus(powerUp.PowerUpType, powerUp.RemoveAfter);
                    powerUp.gameObject.ReturnPooled();
                }
            });
        }

        #region Network Handlers
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

            entity.OnUpdate();
            entity.OnFixedUpdate();
        }

        private void AddEntity(Entity entity)
        {
            gameObject.AddChild(entity.gameObject);
            entities_.Add(entity.ID, entity);
        }

        private void HandleUnitMove(proto_common.Event evt)
        {
            proto_game.UnitMove movePacket = 
                ProtoBuf.Extensible.GetValue<proto_game.UnitMove> (evt, (int)proto_common.Events.UNIT_MOVE);

            Entity unit = GetEntity(movePacket.guid);

            if (unit != null)
            {
                unit.SetNextPosition(movePacket.timestamp, new Vector2(movePacket.x, movePacket.y), movePacket.stop);
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
                playerToInited = player_;
                player_.OnAttack += HandlePlayerAttack;
                player_.OnStop += HandlePlayerStop;
                player_.OnStartMove += HandlePlayerStartMove;
                followCamera.SetTarget(playerToInited.gameObject);
                followCamera.SnapNextTick();
            }

            playerToInited.ID = plrPacket.guid;
            playerToInited.Init(this, new Vector2(plrPacket.position.x, plrPacket.position.y));
            playerToInited.Nickname = plrPacket.name;

            CreateHpBar(playerToInited);

            playerToInited.ApplyStats(plrPacket.stats);
            playerToInited.Health = plrPacket.hp;

            AddEntity(playerToInited);

            players_.Add(playerToInited);

            if (playerToInited == player_)
            {
                player_.Local = true;
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

        private void HandlePlayerStartMove(Entity entity)
        {
            if (entity == player_)
                SendLocalPlayerMoved(false);
        }

        private void HandlePlayerStop(Entity entity)
        {
            if (entity == player_)
                SendLocalPlayerMoved(true);
        }

        private void HandlePlayerAttack(Entity entity)
        {
            var attackRequest = new proto_game.UnitAttack();
            attackRequest.guid = entity.ID;
            attackRequest.direction = entity.transform.rotation.eulerAngles.z;

            GameApp.Instance.Client.Send(attackRequest, proto_common.Commands.ATTACK);
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
                unit.PerformAttack(attPacket.direction);
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

        private void HandleUpgradeStat(proto_game.Stats stat)
        {
            proto_game.StatUpgrade.Request statReq = new proto_game.StatUpgrade.Request();
            statReq.stat = stat;

            int id = GameApp.Instance.Client.Send(statReq, proto_common.Commands.STAT_UPGRADE);
            GameApp.Instance.RequestsManager.AddRequest(id, (proto_common.Response response)=>
            {
                if (response.error != 0)
                {
                    //cancel 1 step
                    arenaUI.StatsPanel.DecreaseLevelOf(stat);
                }
                else
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
            entity.OnRemove();

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