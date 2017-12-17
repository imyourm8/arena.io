using System;
using System.Data;
using System.Text;

using shared.net;
using shared.database;
using shared.helpers;
using shared.security;

using Request = proto_common.Request;
using Response = proto_common.Response;
using Event = proto_common.Event;
using Commands = proto_common.Commands;
using Events = proto_common.Events;
using OperationHandler = shared.net.OperationHandler<proto_common.Request>;

namespace LoginServer
{
    class LoginController : RequestHandler
    {
        private static readonly long LoginTokenLifespanMs = 20000;

        private bool loginInProcess_ = false;

        public LoginController()
        {
            AddOperationHandler(proto_common.Commands.AUTH, new OperationHandler(HandleAuth));
        }

        private async void HandleAuth(proto_common.Request request)
        {
            if (loginInProcess_)
                return;

            var authReq =
                ProtoBuf.Extensible.GetValue<proto_auth.Auth.Request>(request, (int)proto_common.Commands.AUTH);

            if (authReq.m_oauth.uid.Trim() == "")
            {
                SendResponse(proto_common.Commands.AUTH, null, -1);
            }
            else
            {
                var authEntry = new AuthEntry();
                authEntry.authUserID = authReq.m_oauth.uid;
                // currentAuthEntry_ = authEntry;

                var data = await Database.Instance.GetAuthDB().LoginUser(authEntry);
                HandleDBLogin(authEntry, data);
                loginInProcess_ = true;
            }
        }

        private async void HandleDBLogin(AuthEntry authEntry, IDataReader data)
        {
            if (data.Read())
            {
                //create user if no one found
                data = await Database.Instance.GetAuthDB().CreateUser(authEntry);
            }

            if (data != null)
            {
                HandleSuccessfullLogin(authEntry);
            }
            else
            {
                SendLoginFailed();
            }
        }

        private void HandleSuccessfullLogin(AuthEntry authEntry)
        {
            // update login timestamp
            // send ip address of lobby server
            // send unique login token
            var loginToken = new TokenGenerator(LoginTokenLifespanMs).Generate(CurrentTime.Instance.CurrentTimeInMs);
            Database.Instance.GetAuthDB().SetLoginToken(authEntry.authUserID, loginToken.ToString(), (QueryResult result) =>
            {
                // send everything to client
                var response = new proto_auth.Auth.Response();
                response.login_token = loginToken.Value;
                SendResponse(proto_common.Commands.AUTH, response);
            });
        }

        private void SendLoginFailed()
        {
            SendResponse(proto_common.Commands.AUTH, null, 0, (int)proto_common.Common.CommonErrors.CE_ERROR);
        }
    }
}
