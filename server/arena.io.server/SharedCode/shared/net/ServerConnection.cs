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
    public class ServerConnection : OutboundS2SPeer
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private SendParameters defaultSendParams_;

        private string masterIp_;
        private int port_;
        private int connectRetryIntervalMs_;
        private IPEndPoint endpoint_;
        private IFiber fiber_;
        private IDisposable reconnectTimer_;
        private byte isReconnecting_ = 0;

        public ServerConnection(ServerApplication application, string masterIp, int port, int connectRetryIntervalMs)
            : base(application)
        {
            defaultSendParams_.ChannelId = 0;
            defaultSendParams_.Encrypted = false;
            defaultSendParams_.Flush = false;
            defaultSendParams_.Unreliable = true;

            Application = application;
            connectRetryIntervalMs_ = connectRetryIntervalMs;
            masterIp_ = masterIp;
            port_ = port;

            fiber_ = new PoolFiber();
            fiber_.Start();
        }

        public ServerApplication Application { get; private set; }

        public ServerController Controller { private get; set; }

        public void Connect()
        {
            if (reconnectTimer_ != null)
            {
                reconnectTimer_.Dispose();
                reconnectTimer_ = null;
            }

            // check if the photon application is shuting down
            if (this.Application.Running == false)
            {
                return;
            }

            try
            {
                UpdateEndpoint();

                if (log.IsDebugEnabled)
                    log.DebugFormat("MasterServer endpoint for address {0} updated to {1}", this.masterIp_, this.endpoint_);

                if (ConnectTcp(endpoint_, Application.ApplicationName))
                {
                    if (log.IsInfoEnabled)
                        log.InfoFormat("Connecting to master at {0}, serverId={1}", endpoint_, Application.ServerId);
                }
                else
                {
                    if (log.IsWarnEnabled)
                        log.WarnFormat("Connection refused - is the process shutting down ? {0}", this.Application.ServerId);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                if (isReconnecting_ == 1)
                {
                    Reconnect();
                }
                else
                {
                    throw;
                }
            }
        }

        public void UpdateEndpoint()
        {
            IPAddress masterAddress;
            if (!IPAddress.TryParse(masterIp_, out masterAddress))
            {
                var hostEntry = Dns.GetHostEntry(this.masterIp_);
                if (hostEntry.AddressList == null || hostEntry.AddressList.Length == 0)
                {
                    throw new ExitGames.Configuration.ConfigurationException(
                        "MasterIPAddress setting is neither an IP nor an DNS entry: " + this.masterIp_);
                }

                masterAddress = hostEntry.AddressList.First(address => address.AddressFamily == AddressFamily.InterNetwork);

                if (masterAddress == null)
                {
                    throw new ExitGames.Configuration.ConfigurationException(
                        "MasterIPAddress does not resolve to an IPv4 address! Found: "
                        + string.Join(", ", hostEntry.AddressList.Select(a => a.ToString()).ToArray()));
                }
            }

            endpoint_ = new IPEndPoint(masterAddress, port_);
        }

        protected void Reconnect()
        {
            if (this.Application.Running == false)
            {
                return;
            }

            reconnectTimer_ = fiber_.ScheduleOnInterval(() => Connect(), connectRetryIntervalMs_, connectRetryIntervalMs_);
            Thread.VolatileWrite(ref isReconnecting_, 1);
        }

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
            Thread.VolatileWrite(ref isReconnecting_, 0);
        }

        protected override void OnConnectionFailed(int errorCode, string errorMessage)
        {
            if (isReconnecting_ == 0)
            {
                log.ErrorFormat(
                    "Connection failed: address={0}, errorCode={1}, msg={2}",
                    endpoint_,
                    errorCode,
                    errorMessage);
            }
            else if (log.IsWarnEnabled)
            {
                log.WarnFormat(
                    "Master connection failed: address={0}, errorCode={1}, msg={2}",
                    endpoint_,
                    errorCode,
                    errorMessage);
            }

            Reconnect();
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
