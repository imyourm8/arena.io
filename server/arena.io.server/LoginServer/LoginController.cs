using System;
using System.Data;
using System.Text;
using System.Security.Cryptography;

using shared.net;
using shared.database;
using shared.helpers;

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
            var loginToken = GenerateLoginToken();
            var loginExpiryDate = CurrentTime.Instance.CurrentTimeInMs + LoginTokenLifespanMs;
            Database.Instance.GetAuthDB().SetLoginToken(authEntry.authUserID, loginToken, loginExpiryDate, (QueryResult result) =>
            {
                // send everything to client
                var response = new proto_auth.Auth.Response();
                response.login_token = loginToken;
                SendResponse(proto_common.Commands.AUTH, response);
            });
        }

        private string GenerateLoginToken()
        {
            var localTime = CurrentTime.Instance.CurrentTimeInMs.ToString();
            var token = localTime + Guid.NewGuid().ToString();

            var crypt = new SHA256Managed();
            StringBuilder hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(token), 0, Encoding.UTF8.GetByteCount(token));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        private void SendLoginFailed()
        {
            SendResponse(proto_common.Commands.AUTH, null, 0, (int)proto_common.Common.CommonErrors.CE_ERROR);
        }
    }
}
