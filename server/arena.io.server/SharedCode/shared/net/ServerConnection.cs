using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;

using Photon.SocketServer;
using ExitGames.Concurrency.Fibers;
using PhotonHostRuntimeInterfaces;
using Photon.SocketServer.ServerToServer;
using ProtoBuf;
using ExitGames.Logging;

using Response = proto_server.Response;
using Request = proto_server.Request;
using Event = proto_server.Event;

namespace shared.net
{
    using interfaces;

    public class ServerConnection : OutboundS2SPeer, IConnection<ServerController>
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private SendParameters defaultSendParams_;

        public ServerConnection(ServerApplication application)
            : base(application)
        {
            defaultSendParams_.ChannelId = 0;
            defaultSendParams_.Encrypted = false;
            defaultSendParams_.Flush = false;
            defaultSendParams_.Unreliable = true;

            Application = application;
        }

        #region Properties

        public ServerApplication Application { get; private set; }
        protected ServerController Controller { get; set; }

        #endregion

        #region IConnection implementation

        public void SetController(ServerController controller)
        {
            Controller = controller;
        }

        #endregion

        #region Send Routine

        public void Send(Response response)
        {
            Send(response, defaultSendParams_);
        }

        public void Send(Response response, SendParameters sendParameters)
        {
            if (!Connected)
                return;
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, response);
                stream.Flush();
                stream.Position = 0;
                var parameters = new Dictionary<byte, object>();
                parameters[OperationParameters.ProtoData] = stream.ToArray();
                var opResponse = new OperationResponse
                {
                    OperationCode = OperationParameters.ProtoCmd,
                    ReturnCode = 0,
                    DebugMessage = "OK",
                    Parameters = parameters
                };

                var result = SendOperationResponse(opResponse, sendParameters);
                if (result != SendResult.Ok)
                {
                    log.Error("Response send failed with result code: " + result.ToString() + ". Response type " + response.type);
                }
                //Flush();
            }
        }

        public void Send(Request request, SendParameters sendParameters)
        {
            if (!Connected)
                return;
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, request);
                stream.Flush();
                stream.Position = 0;
                var parameters = new Dictionary<byte, object>();
                parameters[OperationParameters.ProtoData] = stream.ToArray();
                var opRequest = new OperationRequest
                {
                    OperationCode = OperationParameters.ProtoCmd,
                    Parameters = parameters
                };

                var result = SendOperationRequest(opRequest, sendParameters);
                if (result != SendResult.Ok)
                {
                    log.Error("Request send failed with result code: " + result.ToString());

                }
                //Flush();
            }
        }

        public void Send(Request request)
        {
            Send(request, defaultSendParams_);
        }

        public void Send(Event evt)
        {
            Send(evt, defaultSendParams_);
        }

        public void Send(Event evt, SendParameters sendParameters)
        {
            if (!Connected)
                return;
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, evt);
                stream.Flush();
                stream.Position = 0;
                var parameters = new Dictionary<byte, object>();
                parameters[OperationParameters.ProtoData] = stream.ToArray();
                var evtData = new EventData
                {
                    Code = OperationParameters.ProtoCmd,
                    Parameters = parameters
                };

                var result = SendEvent(evtData, sendParameters);
                if (result != SendResult.Ok)
                {
                    log.Error("Event send failed with result code: " + result.ToString() + ". Event type " + evt.type);
                }
                // Flush();
            }
        }
#endregion

#region OutboundS2SPeer implementation
        protected override void OnConnectionEstablished(object responseObject)
        {
            
        }

        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        {
            
        }

        protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
        {
            Event evt;
            byte[] data = (byte[])eventData.Parameters[OperationParameters.ProtoData];
            using (var stream = new MemoryStream(data))
            {
                evt = ProtoBuf.Serializer.Deserialize<Event>(stream);
            }

            try
            {
                Controller.HandleEvent(evt);
            }
            catch (System.Exception exc)
            {
                log.Error(exc.Message);
            }
        }

        protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            Response response;
            byte[] data = (byte[])operationResponse.Parameters[OperationParameters.ProtoData];
            using (var stream = new MemoryStream(data))
            {
                response = ProtoBuf.Serializer.Deserialize<Response>(stream);
            }

            try
            {
                Controller.HandleResponse(response);
            }
            catch (System.Exception exc)
            {
                log.Error(exc.Message);
                Disconnect();
            }
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            Controller.HandleDisconnect();
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Request request;
            byte[] data = (byte[])operationRequest.Parameters[OperationParameters.ProtoData];
            using (var stream = new MemoryStream(data))
            {
                request = ProtoBuf.Serializer.Deserialize<Request>(stream);
            }

            try
            {
                Controller.HandleRequest(request);
            }
            catch (System.Exception exc)
            {
                log.Error(exc.Message);
                Disconnect();
            }
        }
#endregion
    }
}
