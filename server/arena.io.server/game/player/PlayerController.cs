using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using ExitGames.Concurrency.Fibers;
using arena.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace arena.player
{
    class PlayerController : RequestHandler, player.IActionInvoker
    {
        private TapCommon.ClientState state_ = TapCommon.ClientState.Unlogged;
        private battle.Player player_;
        private PoolFiber fiber_ = new PoolFiber();
        private bool loginInProcess_ = false;
        private AuthEntry currentAuthEntry_ = null;

        public PlayerController()
        {
            AddOperationHandler(proto_common.Commands.AUTH, new OperationHandler(HandleAuth));
            AddOperationHandler(proto_common.Commands.PING, new OperationHandler(HandlePing));
            AddOperationHandler(proto_common.Commands.PLAYER_MOVE, new OperationHandler(HandlePlayerMove));
            AddOperationHandler(proto_common.Commands.ATTACK, new OperationHandler(HandlePlayerAttack));
            AddOperationHandler(proto_common.Commands.TURN, new OperationHandler(HandlePlayerTurn));
            AddOperationHandler(proto_common.Commands.DAMAGE, new OperationHandler(HandlePlayerDamage));
            AddOperationHandler(proto_common.Commands.CHANGE_NICKNAME, new OperationHandler(HandleChangeNickname));
            AddOperationHandler(proto_common.Commands.JOIN_GAME, new OperationHandler(HandleJoinGame));
            AddOperationHandler(proto_common.Commands.FIND_ROOM, new OperationHandler(HandleFindRoom));

            fiber_.Start();
        }

        protected override IActionInvoker GetActionInvoker()
        {
            if (player_ != null && player_.Game != null)
            {
                return player_.Game;
            }

            return this;
        }

        private void Send(proto_common.Event evt)
        {
            if (connection_ != null)
            {
                connection_.Send(evt);
            }
        }

        public void SendResponse(proto_common.Commands cmd, object data, int id = 0, int error = 0)
        {
            var response = new proto_common.Response();
            response.type = cmd;
            response.id = id;
            response.error = error;
            response.timestamp = helpers.CurrentTime.Instance.CurrentTimeInMs;

            ProtoBuf.Extensible.AppendValue(response, (int)cmd, data);
            connection_.Send(response);
        }

        public void SendEvent(proto_common.Events evtCode, object data)
        {
            var evt = new proto_common.Event();
            evt.type = evtCode;
            evt.timestamp = helpers.CurrentTime.Instance.CurrentTimeInMs;

            ProtoBuf.Extensible.AppendValue(evt, (int)evtCode, data);
            Send(evt);
        }

        public override void HandleDisconnect()
        {
            base.HandleDisconnect();

            if (player_ != null)
            {
                battle.RoomManager.Instance.RemovePlayer(player_);
            }

            state_ &= ~TapCommon.ClientState.InBattle;
            fiber_.Stop();
        }

        #region Request Handlers
        public override bool FilterRequest(proto_common.Request request)
        {
            bool filtered = false;
            bool alwaysExecute = false;
            TapCommon.OperationCondition condition;
            if (TapCommon.OperationCondition.conditionList.TryGetValue((int)request.type, out condition))
            {
                //iterate through every possible state
                foreach (TapCommon.ClientState state in Enum.GetValues(typeof(TapCommon.ClientState)))
                {
                    //if state required in condition call 
                    if ((condition.State & state) != 0)
                    {
                        //and exists in current state
                        if ((state_ & state) == 0)
                        {
                            filtered |= true;
                        }
                    }
                }

                if (condition.Execution == TapCommon.OperationCondition.ExecutionMethod.AlwaysExecute)
                {
                    alwaysExecute = true;
                }
            }

            return filtered && !alwaysExecute;
        }

        private void HandleChangeNickname(proto_common.Request request)
        {
            if (player_ == null)
            {
                return;
            }
            var changeNickReq = request.Extract<proto_profile.ChangeNickname.Request>(proto_common.Commands.CHANGE_NICKNAME);

            var name = changeNickReq.name.Trim();
            var nickResponse = new proto_profile.ChangeNickname.Response();

            if (name != player_.Name)
            {
                Database.Database.Instance.GetPLayerDB().ChangeNickname(player_.UniqueID, name, (QueryResult result) =>
                    {
                        if (result == QueryResult.Success)
                        {
                            nickResponse.success = true;
                            SendResponse(proto_common.Commands.CHANGE_NICKNAME, nickResponse);
                        }
                    });
            }
            else
            {
                nickResponse.success = false;
                SendResponse(proto_common.Commands.CHANGE_NICKNAME, nickResponse);
            }
        }

        private void HandleAuth(proto_common.Request request)
        {
            if (loginInProcess_)
            {
                return;
            }

            var authReq =
                ProtoBuf.Extensible.GetValue<proto_auth.Auth.Request>(request, (int)proto_common.Commands.AUTH);

            var authEntry = new AuthEntry();
            authEntry.authUserID = authReq.m_oauth.uid;
            currentAuthEntry_ = authEntry;

            Database.Database.Instance.GetAuthDB().LoginUser(authEntry, HandleDBLogin);
            loginInProcess_ = true;
        }

        private void HandleDBLogin(QueryResult result, IDataReader data)
        {
            if (data.Read())
            {
                LoadUserFromDB(data);
            }
            else
            {
                //create user if no one found
                Database.Database.Instance.GetAuthDB().CreateUser(currentAuthEntry_, HandleCreateUser);
            }
        }

        private void LoadUserFromDB(IDataReader data)
        {
            var authRes = new proto_auth.Auth.Response();
            loginInProcess_ = false;

            player_ = new battle.Player(this);
            player_.Name = (string)data["name"];
            player_.UniqueID = (string)data["authUserID"];

            proto_profile.UserInfo info = new proto_profile.UserInfo();
            info.coins = (int)data["coins"];
            info.level = (int)data["level"];
            info.name = player_.Name;

            var reader = new JsonTextReader(new StringReader((string)data["unlocked_classes"]));
            HashSet<proto_profile.PlayerClasses> unclockedClasses = new HashSet<proto_profile.PlayerClasses>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.EndArray) continue;
                //json looks like array of classes ids
                unclockedClasses.Add(helpers.Parsing.ParseEnum<proto_profile.PlayerClasses>((string)reader.Value));
            }

            foreach (var entry in Factories.PlayerClassFactory.Instance.GetAllClasses())
            {
                proto_profile.ClassInfo clInfo = new proto_profile.ClassInfo();
                clInfo.@class = entry.Value.Class;
                clInfo.coinsPrice = entry.Value.Price;
                clInfo.levelRequired = entry.Value.MinLevel;
                clInfo.unlocked = unclockedClasses.Contains(clInfo.@class);
            }

            authRes.info = info;
            state_ = TapCommon.ClientState.Logged;
            SendResponse(proto_common.Commands.AUTH, authRes);
        }

        private void HandleCreateUser(QueryResult result, IDataReader data)
        {
            if (data.Read())
            {
                LoadUserFromDB(data);
            }
            else
            {
                //something gone wrong
                var authRes = new proto_auth.Auth.Response();
                SendResponse(proto_common.Commands.AUTH, authRes, 0, (int)proto_common.Common.CommonErrors.CE_ERROR);
            }
        }

        private void HandleFindRoom(proto_common.Request request)
        {
            battle.RoomManager.Instance.AssignPlayerToRandomRoom(player_);

            state_ |= TapCommon.ClientState.SwitchGameServer;
        }

        private void HandleJoinGame(proto_common.Request request)
        {
            var joinReq = request.Extract<proto_game.JoinGame.Request>(proto_common.Commands.JOIN_GAME);

            player_.SelectedClass = joinReq.@class;
            player_.Game.PlayerJoin(player_);

            state_ &= ~TapCommon.ClientState.SwitchGameServer;
            state_ |= TapCommon.ClientState.InBattle;
        }

        private void HandlePing(proto_common.Request request)
        {
            var ping =
               ProtoBuf.Extensible.GetValue<proto_game.Ping.Request>(request, (int)proto_common.Commands.PING);

            var pong = new proto_game.Ping.Response();
            pong.timestamp = helpers.CurrentTime.Instance.CurrentTimeInMs;

            SendResponse(proto_common.Commands.PING, pong, request.id);
        }

        private void HandlePlayerMove(proto_common.Request request)
        {
            var moveRequest = request.Extract<proto_game.PlayerMove.Request>(proto_common.Commands.PLAYER_MOVE);
            player_.Game.PlayerMoved(player_, moveRequest);
        }

        private void HandlePlayerAttack(proto_common.Request request)
        {
            var attackRequest = request.Extract<proto_game.UnitAttack>(proto_common.Commands.ATTACK);

            player_.Game.PlayerAttacked(player_, attackRequest);
        }

        private void HandlePlayerTurn(proto_common.Request request)
        {
            var turnRequest = request.Extract<proto_game.PlayerTurn>(proto_common.Commands.TURN);
            player_.Game.PlayerTurned(player_, turnRequest);
        }

        private void HandlePlayerDamage(proto_common.Request request)
        {
            var damageRequest = request.Extract<proto_game.ApplyDamage>(proto_common.Commands.DAMAGE);
            player_.Game.UnitDamaged(player_, damageRequest);
        }

        #endregion

        void IActionInvoker.Execute(Action action)
        {
            fiber_.Enqueue(action);
        }
    }
}
