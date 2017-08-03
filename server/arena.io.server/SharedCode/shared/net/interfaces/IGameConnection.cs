﻿using Photon.SocketServer;

namespace shared.net.interfaces
{
    public interface IGameConnection
    {
        void SetController(RequestHandler controller);
        void Send(proto_common.Response response);
        void Send(proto_common.Response response, SendParameters sendParameters);
        void Send(proto_common.Event evt);
        void Disconnect();
    }
}