using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using UnityEngine;
using proto_common;

public class Packet
{
	private byte[] data_;
	
	public byte[] Data
	{
		get { return data_; }
	}

	static public Packet FromProto(ref proto_common.Request request)
	{
		Packet packet = new Packet ();
		ProtoSerializer serializer = new ProtoSerializer ();
		using (MemoryStream stream = new MemoryStream()) {
			serializer.Serialize(stream, request);
			stream.Flush();
			stream.Position = 0;
			packet.data_ = stream.ToArray();
		}
		return packet;
	}
}

public class NetworkManager
{
	private static int MAX_MESSAGE_SIZE = 16000;
	private static int HEADER_SIZE = 7;
	private static int PROTOCOL_INDEX = 0;
	private static int FLAGS_INDEX = 1;
	private static int DATA_LENGTH_INDEX = 5;
	private static int DATA_INDEX = 7;
	private static int PROTOCOL_VERSION = 1;

	public delegate void ConnectionCallback(bool isConnected);
	public delegate void RecieveCallback(proto_common.Response response);
	public delegate void RecieveEventCallback(proto_common.Event evt);

	public event RecieveEventCallback RecieveEventEvent;
	public event RecieveCallback RecieveResponseEvent;

	private EndPoint ipReciever_;
	private int bodySize_ = 0;
	private bool sendInProcess_ = false;
	private bool isEvent = false;
	private Queue packetQueue_ = new Queue();
	private ConnectionCallback connectionCb_;
	private static NetworkManager instance_ = new NetworkManager();
	private Socket clientSocket_ = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
	private byte[] recieveBuffer_ = new byte[MAX_MESSAGE_SIZE];
    private long totalBytesReceived_ = 0; 

	private NetworkManager() 
	{
		Debug.Log ("Created Network Manager Instance");
		clientSocket_.Blocking = false;
	}

	public static NetworkManager Instance
	{
		get { return instance_; }
	}

	public void Connect(string host, int port, ConnectionCallback cb)
	{
		try {
			connectionCb_ = cb;
			IPAddress address;
			if (!IPAddress.TryParse(host, out address))
			{
				throw new FormatException("Wrong host string provided!");
			}
			ipReciever_ = new IPEndPoint(address, port);
			clientSocket_.BeginConnect(ipReciever_, new AsyncCallback(HandleConnect), clientSocket_);
		}
		catch (SocketException e)
		{
		}
		catch (Exception e)
		{
		}
	}

	private void HandleConnect(IAsyncResult ar)
	{
		clientSocket_.EndConnect (ar);
		connectionCb_ (ar.IsCompleted);
		if (ar.IsCompleted)
		{
			RecieveHeader();
		}
	}

	private void RecieveHeader()
	{
		clientSocket_.BeginReceiveFrom (recieveBuffer_, 0, HEADER_SIZE, SocketFlags.None, ref ipReciever_, 
		                                new AsyncCallback (HandleRecieveHeader), null);
	}

	private void HandleRecieveHeader(IAsyncResult ar)
	{
		int recieved = clientSocket_.EndReceive (ar);

        totalBytesReceived_ += recieved;

		if (recieved == HEADER_SIZE)
		{
			isEvent = BitConverter.ToInt32(recieveBuffer_, FLAGS_INDEX) == (int)proto_common.PacketFlags.EVENT;
			bodySize_ = BitConverter.ToInt16(recieveBuffer_, DATA_LENGTH_INDEX);
			RecieveBody();
		}
	}

	private void RecieveBody()
	{
		clientSocket_.BeginReceiveFrom (recieveBuffer_, 0, bodySize_, SocketFlags.None, ref ipReciever_, 
		                                new AsyncCallback (HandleRecieveBody), null);
	}

	private void HandleRecieveBody(IAsyncResult ar)
	{
		int recieved = clientSocket_.EndReceive (ar);
        totalBytesReceived_ += recieved;

		if (recieved == bodySize_)
		{
			if (isEvent) {
				ProtoSerializer serializer = new ProtoSerializer();
				proto_common.Event evt = null;
				
				using (MemoryStream stream = new MemoryStream(recieveBuffer_,0,bodySize_,false))
				{
					try {
						evt = (proto_common.Event)serializer.Deserialize(stream, evt, typeof(proto_common.Event));
					} catch (Exception e)
					{
						Debug.Log(e.Message);
					}
				}
				Worker instance = Worker.Instance;
				instance.Add(delegate { 
					if (RecieveEventEvent != null) {
						RecieveEventEvent(evt);
						RecieveHeader(); 
					}
					return RecieveEventEvent != null;
				});
			} else {
				ProtoSerializer serializer = new ProtoSerializer();
				proto_common.Response response = null;

				using (MemoryStream stream = new MemoryStream(recieveBuffer_,0,bodySize_,false))
				{
					try {
					response = (proto_common.Response)serializer.Deserialize(stream, response, typeof(proto_common.Response));
					} catch (Exception e)
					{
						Debug.Log(e.Message);
					}
				}
				Worker.Instance.Add(delegate { 
					if (RecieveResponseEvent != null) {
						RecieveResponseEvent(response);
						RecieveHeader(); 
					}
					return RecieveResponseEvent != null;
				});
			}
		} else 
		{
			Debug.LogError("Body has been not recieved properly!");
		}
	}

	public void Send(Packet packet)
	{
		packetQueue_.Enqueue (packet);
		SendNextPacket ();
	}

	private void SendNextPacket()
	{
		if (sendInProcess_ || packetQueue_.Count == 0)
		{
			return;
		}

		if (!clientSocket_.Connected)
		{
			throw new Exception("Socket is closed!");
		}

		sendInProcess_ = true;
		Packet packet = (Packet)packetQueue_.Dequeue ();
		byte[] data = packet.Data;
		//create header
		byte[] dataWithHeader = new byte[data.Length + HEADER_SIZE];
		dataWithHeader [PROTOCOL_INDEX] = (byte)PROTOCOL_VERSION;
		Array.Copy (BitConverter.GetBytes ((int)0), 0, dataWithHeader, FLAGS_INDEX, 4); //int 4 bytes
		Array.Copy (BitConverter.GetBytes ((short)data.Length), 0, dataWithHeader, DATA_LENGTH_INDEX, 2);//short 2 bytes
		Array.Copy (data, 0, dataWithHeader, DATA_INDEX, data.Length);
		clientSocket_.BeginSend(dataWithHeader, 0, dataWithHeader.Length, SocketFlags.None, 
		                        new AsyncCallback(HandlePacketSend), clientSocket_);
	}

	private void HandlePacketSend(IAsyncResult ar)
	{
		sendInProcess_ = false;
		int bytesSent = clientSocket_.EndSend (ar);
		Debug.Log ("Sent " + bytesSent.ToString ());
		SendNextPacket ();
	}
}
