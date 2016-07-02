using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ProtoBuf;
using proto_common;

public class ServerClient  : IPhotonPeerListener {
	public delegate void ResponseDelegate (proto_common.Response response);
	public event ResponseDelegate OnServerResponse;

	public delegate void EventDelegate (proto_common.Event evt);
	public event EventDelegate OnServerEvent;

	public delegate void StatusDelegate (StatusCode status);
	public event StatusDelegate OnStatusChange;

	private Queue eventQueue = new Queue();
	private Queue responseQueue = new Queue();
	private ServerConnection connection_;
	private string address_;
	private StatusCode status_;

	public ServerClient(string address, ConnectionProtocol protocol = ConnectionProtocol.Tcp) 
    {
		connection_ = new ServerConnection (this, protocol);
		address_ = address;
	}

    public long TotalBytesReceived
    { get; set; }

	public void Service() {
		if (connection_ != null) {
			connection_.Service();
		}

		while (OnServerResponse != null && responseQueue.Count > 0) {
			var response = (proto_common.Response)responseQueue.Dequeue();
			OnServerResponse(response);
		}

		while (OnServerEvent != null && eventQueue.Count > 0) {
			var evt = (proto_common.Event)eventQueue.Dequeue();
			//if (Math.Abs(ClickerApp.Instance.TimeMs() - evt.timestamp) > 4000) continue;

			OnServerEvent(evt);
		}
	}

	public bool Connect() {
		return connection_.Connect (address_, "");
	}

    public void Send(object msg, proto_common.Commands cmd)
    {
        proto_common.Request req = new proto_common.Request ();
        req.type = cmd;
        req.id = 0;

        ProtoBuf.Extensible.AppendValue (req, (int)cmd, msg);

        Send(req);
    }

	public bool Send(proto_common.Request req, bool reliable=true) 
    {
		ProtoSerializer serializer = new ProtoSerializer ();
		bool result = false;
		using (MemoryStream stream = new MemoryStream()) {
			serializer.Serialize(stream, req);
			stream.Flush();
			stream.Position = 0;
			var dict = new Dictionary<byte, object>();
			dict[TapCommon.OperationParameters.ProtoData] = stream.ToArray();
			result = connection_.OpCustom(TapCommon.OperationParameters.ProtoCmd, dict, reliable);
		}
		return result;
	}

	public StatusCode Status {
		get {return status_; }
	}

	#region IPhotonPeerListener implementation

	public void DebugReturn (DebugLevel level, string message) {
		if (level == DebugLevel.ERROR) {
			Debug.LogError(message);
		} else {
			Debug.Log(message);
		}
	}

	public void OnOperationResponse (OperationResponse operationResponse) {
		ProtoSerializer serializer = new ProtoSerializer();
		proto_common.Response response = null;

		byte[] protoData = (byte[])operationResponse[TapCommon.OperationParameters.ProtoData];
		using (MemoryStream stream = new MemoryStream(protoData,0,protoData.Length,false))
		{
			try {
				response = (proto_common.Response)serializer.Deserialize(stream, response, typeof(proto_common.Response));
			} catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}
		responseQueue.Enqueue (response);
	}

	public void OnStatusChanged (StatusCode statusCode) {
		status_ = statusCode;
		if (OnStatusChange != null) {
			OnStatusChange(statusCode);
		}
	}

	public void OnEvent (EventData eventData) {
		ProtoSerializer serializer = new ProtoSerializer();
		proto_common.Event evt = null;

		byte[] protoData = (byte[])eventData[TapCommon.OperationParameters.ProtoData];
		using (MemoryStream stream = new MemoryStream(protoData,0,protoData.Length,false))
		{
			try {
				evt = (proto_common.Event)serializer.Deserialize(stream, evt, typeof(proto_common.Event));
			} catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}

        TotalBytesReceived += protoData.Length;
		eventQueue.Enqueue (evt);
	}
	
	#endregion
}
