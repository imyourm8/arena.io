using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ServerConnection : PhotonPeer {

	public ServerConnection(IPhotonPeerListener listener, ConnectionProtocol protocolType) : 
		base(listener, protocolType)
	{
	}
}
