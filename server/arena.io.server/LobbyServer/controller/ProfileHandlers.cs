using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using shared.database;

using Request = proto_common.Request;
using Response = proto_common.Response;
using Event = proto_common.Event;
using Commands = proto_common.Commands;
using Events = proto_common.Events;

namespace LobbyServer.controller
{
    partial class LobbyController
    {
        private void HandleChangeNickname(Request request)
        {
            if (profile_ == null)
            {
                return;
            }
            var changeNickReq = request.Extract<proto_profile.ChangeNickname.Request>(proto_common.Commands.CHANGE_NICKNAME);

            var name = changeNickReq.name.Trim();
            var nickResponse = new proto_profile.ChangeNickname.Response();

            if (name != profile_.Name)
            {
                Database.Instance.GetPLayerDB().ChangeNickname(profile_.UniqueID, name, (QueryResult result) =>
                {
                    if (result == QueryResult.Success)
                    {
                        nickResponse.success = true;
                        SendResponse(request, nickResponse);
                    }
                });
            }
            else
            {
                nickResponse.success = false;
                SendResponse(request, nickResponse);
            }
        }
    }
}
