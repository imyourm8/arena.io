using System.Data;

using shared;
using shared.net;
using shared.database;
using shared.helpers;
using shared.account;

namespace LobbyServer.controller
{
    class LobbyController : RequestHandler
    {
        private bool loginInProcess_ = false;
        private Profile profile_ = null;
        private ClientState state_ = ClientState.Unlogged;

        public LobbyController()
        {
            AddOperationHandler(proto_common.Commands.CONNECT_TO_LOBBY, new OperationHandler(HandleConnection));
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

            // validate login token
            var authEntry = new AuthEntry();
            authEntry.authUserID = conRequest.uid;
            var result = await Database.Instance.GetAuthDB().LoginUser(authEntry);
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
            if (!loginToken.Equals(dbToken))
            {
                Connection.Disconnect();
            }
            else
            {
                string[] tokenParts = loginToken.Split(':');
                long expiryDate = long.Parse(tokenParts[1]);

                if (expiryDate >= CurrentTime.Instance.CurrentTimeInMs)
                {
                    LoadUser(userData);
                }
                else
                {
                    Connection.Disconnect();
                }
            }
        }

        private void LoadUser(IDataReader data)
        {
            var authRes = new proto_auth.ConnectToLobby.Response();

            profile_ = new Profile(data);
            var info = profile_.GetInfoPacket();
            authRes.info = info;

            state_ = ClientState.Logged;
            SendResponse(proto_common.Commands.AUTH, authRes);
        }
#endregion
    }
}
