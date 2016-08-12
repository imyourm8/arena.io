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
    private int reqId_ = 0;

	public ServerClient(string address, ConnectionProtocol protocol = ConnectionProtocol.Tcp) 
    {
		connection_ = new ServerConnection (this, protocol);
        address_ = address;
	}

    public long TotalBytesReceived
    {  
        get { return connection_.BytesIn; }
    }

    public int Latency
    {
        get { return connection_.RoundTripTime; }
    }

    public long ServerTime
    {
        get { return connection_.ServerTimeInMilliSeconds; }
    }

    public NetworkSimulationSet NetworkSettings
    {
        get { return connection_.NetworkSimulationSettings; }
    }

	public void Service() 
    {
		if (connection_ != null) 
        {
			connection_.Service();
		}

		while (OnServerResponse != null && responseQueue.Count > 0) 
        {
			var response = (proto_common.Response)responseQueue.Dequeue();
			OnServerResponse(response);
		}

		while (OnServerEvent != null && eventQueue.Count > 0) 
        {
			var evt = (proto_common.Event)eventQueue.Dequeue();
			OnServerEvent(evt);
		}
	}

	public bool Connect() 
    {
		var result = connection_.Connect (address_, "arena.io.client");
        connection_.IsSimulationEnabled = true;
        return result;
	}

    //returns request id
    public int Send(object msg, proto_common.Commands cmd)
    {
        proto_common.Request req = new proto_common.Request ();
        req.type = cmd;
        req.id = reqId_++;

        ProtoBuf.Extensible.AppendValue (req, (int)cmd, msg);

        Send(req);

        return req.id;
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
		if (OnStatusChange != null) 
        {
			Worker.Instance.Add(()=>
            {
                OnStatusChange(statusCode);
                return true;
            });
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

		eventQueue.Enqueue (evt);
	}
	
	#endregion
}
