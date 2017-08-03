﻿using System.Data;

using shared;
using shared.net;
using shared.database;
using shared.helpers;
using shared.account;

using Request = proto_common.Request;
using Response = proto_common.Response;
using Event = proto_common.Event;
using Commands = proto_common.Commands;
using Events = proto_common.Events;
using OperationHandler = shared.net.OperationHandler<proto_common.Request>;

namespace LobbyServer.controller
{
    partial class LobbyController : RequestHandler
    {
        private bool loginInProcess_ = false;
        private Profile profile_ = null;

        public LobbyController()
        {
            AddOperationHandler(Commands.CONNECT_TO_LOBBY, new OperationHandler(HandleConnection));
            AddOperationHandler(Commands.CHANGE_NICKNAME, new OperationHandler(HandleChangeNickname));
            AddOperationHandler(Commands.JOIN_GAME, new OperationHandler(HandleJoinGame));
            AddOperationHandler(Commands.FIND_ROOM, new OperationHandler(HandleFindRoom));
        }

        public override bool FilterRequest(proto_common.Request request)
        {
            bool filtered = true;

            return filtered;
        }

        public async void HandleConnection(proto_common.Request request)
        {
            if (loginInProcess_)
            {
                Connection.Disconnect();
                return;
            }

            proto_auth.ConnectToLobby.Request conRequest =
                request.Extract<proto_auth.ConnectToLobby.Request>(proto_common.Commands.CONNECT_TO_LOBBY);

            // get user fields
            var authEntry = new AuthEntry();
            authEntry.authUserID = conRequest.uid;
            var result = await Database.Instance.GetAuthDB().LoginUser(authEntry);
            // currently login token is a part of user table, so we just grab whole data
            // upon successfull token check load user without further DB requests
            // user found, validate login token
            if (result != null)
            {
                ValidateLoginToken(result, conRequest.login_token);
            }
            else
            {
                Connection.Disconnect();
            }
        }

#region Private Methods
        private void ValidateLoginToken(IDataReader userData, string loginToken)
        {
            userData.Read();

            string dbToken = (string)userData["login_token"];
            string[] tokenParts = loginToken.Split(':');
            long expiryDate = long.Parse(tokenParts[1]);

            if (expiryDate >= CurrentTime.Instance.CurrentTimeInMs || !loginToken.Equals(dbToken))
            {
                Connection.Disconnect();
            }
            else
            {
                LoadUser(userData);
            }
        }

        private void LoadUser(IDataReader data)
        {
            var authRes = new proto_auth.ConnectToLobby.Response();

            profile_ = new Profile(data);
            var info = profile_.GetInfoPacket();
            authRes.info = info;

            ResetState(ClientState.Logged);
            SendResponse(proto_common.Commands.AUTH, authRes);
        }
#endregion
    }
}