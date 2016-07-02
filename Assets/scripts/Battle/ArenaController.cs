using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System.Timers;

namespace arena
{
    public class ArenaController : MonoBehaviour 
    {
        [System.Serializable]
        struct PlayerLevelData
        {
            public AnimatedProgress progressBar;
            public Text progressText;
        }

        [Header("Arena Set-Up")]
        [SerializeField]
        private Joystick moveJoystick_ = null;

        [SerializeField]
        private Joystick shootJoystick_ = null;

        [SerializeField]
        private GameObject playerPrefab = null;

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
        private LoginSceneController loginScene = null;

        [SerializeField]
        private ExpBlocksDict blockPrefabs = null;

        [SerializeField]
        private PlayerLevelData plrLevelData;

        [SerializeField]
        private StatsPanel statsPanel = null;

        private Player player_ = null;

        private Timer positionTimer_;
        private Dictionary<int, Entity> entities_ = new Dictionary<int, Entity>();

        private void Start()
        {
            GameApp.Instance.Client.OnServerEvent += HandleEvent;

            foreach(var entity in entities_)
            {
                entity.Value.gameObject.Detach();
                Destroy(entity.Value.gameObject);
            }

            entities_.Clear();

            plrLevelData.progressBar.ShowSmooth();
            plrLevelData.progressBar.Progress = 0.0f;
            plrLevelData.progressText.text = "1";
        }

        private void OnGameJoin()
        {
            positionTimer_ = new Timer(1000 / GameApp.Instance.MovementUpdateFrequency);
            positionTimer_.Elapsed += (object sender, ElapsedEventArgs e) => 
            {
                Worker.Instance.Add(NotifyServerAboutPlayerPosition);
            };
            positionTimer_.Start();

            player_ = CreatePlayer();
            player_.Local = true;
        }

        private Player CreatePlayer()
        {
            var player = playerPrefab.GetPooled();

            var playerScript = player.GetComponent<Player>();
            playerScript.PlayerExperience.OnLevelUp = HandleLevelUp;

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
            if (player_ == null) return;

    	    Vector3 dir = Vector3.zero;

            dir.x = Input.GetAxis("Horizontal");
            dir.y = Input.GetAxis("Vertical");

            dir = dir.normalized;

            if (moveJoystick_.inputDirection != Vector2.zero)
            {
                dir = moveJoystick_.inputDirection;
            }

            player_.Force = dir;
            player_.OnUpdate();

            if (shootJoystick_.inputDirection != Vector2.zero)
            {
                player_.PerformAttack(shootJoystick_.inputDirection);
            }
    	}

        private bool NotifyServerAboutPlayerPosition()
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

            return true;
        }

        private long prevTime_ = 0;
        private Vector2 prevPos;
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
            } else if (evt.type == proto_common.Events.PLAYER_APPEARED)
            {
                HandlePlayerAppeared(evt);
            } else if (evt.type == proto_common.Events.PLAYER_DISCONNECTED)
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
        }
        #endregion

        #region Network Handlers
        private void HandlePlayerExperience(proto_common.Event evt)
        {
            var expPacket = evt.Extract<proto_game.PlayerExperience>(proto_common.Events.PLAYER_EXPERIENCE);

            player_.PlayerExperience.AddExperience(expPacket.exp);

            plrLevelData.progressBar.Progress = player_.PlayerExperience.ExpProgress;
            plrLevelData.progressText.text = player_.Level.ToString();
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
                playerToInited = CreatePlayer();
            }
            else 
            {
                playerToInited = player_;
                player_.OnAttack += HandlePlayerAttack;
                player_.OnStop += HandlePlayerStop;
                player_.OnStartMove += HandlePlayerStartMove;
                followCamera.SnapNextTick();
            }

            playerToInited.ID = plrPacket.guid;
            playerToInited.Init(this, new Vector2(plrPacket.position.x, plrPacket.position.y));
            playerToInited.Nickname = plrPacket.name;

            CreateHpBar(playerToInited);

            playerToInited.Health = plrPacket.hp;
            playerToInited.ApplyStats(plrPacket.stats);
            AddEntity(playerToInited);
        }

        private void CreateHpBar(Entity entity)
        {
            var hpBar = hpBarsPrefab.GetPooled().GetComponent<AnimatedProgress>();
            entity.HpBar = hpBar;
            hpBar.HideSmooth();
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
                DestroyEntity(entity);
            }
        }

        #endregion

        private void HandleLevelUp(int level)
        {
            statsPanel.AddPoints(1);
        }

        private void DestroyEntity(int who)
        {
            Entity entity = GetEntity(who);
            if (entity != null && entity != player_)
            {
                DestroyEntity(entity);
            }
            else if (entity == player_)
            {
                loginScene.SwitchArenaView(false);
            }
        }

        private void DestroyEntity(Entity entity)
        {
            entity.OnRemove();
            entities_.Remove(entity.ID);

            if (entity == player_)
            {
                loginScene.SwitchArenaView(false);
            }
            else
            {
                entity.gameObject.ReturnPooled();
            }
        }

        private void OnDisable() 
        {
            GameApp.Instance.Client.OnServerEvent -= HandleEvent;

            if (positionTimer_ != null)
                positionTimer_.Stop();

            if (player_ != null)
                player_.OnAttack -= HandlePlayerAttack;
        }
    }
}