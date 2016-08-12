using UnityEngine;
using System.Collections;

public class NetworkSimulation : MonoBehaviour 
{	
    [SerializeField]
    private int IncomingLag = 0;

    [SerializeField]
    private int IncomingJitter = 0;

    [SerializeField]
    private int IncomingPacketLoss = 0;

    [SerializeField]
    private int OutgoinLag = 0;

    [SerializeField]
    private int OutgoingJitter = 0;

    [SerializeField]
    private int OutgoingPacketLoss = 0;

    private ServerClient client_;

	void Update () 
    {
        if (client_ == null)
        {
            client_ = GameApp.Instance.Client;
        }
        else 
        {
            var settings = client_.NetworkSettings;

            settings.IncomingJitter = IncomingJitter;
            settings.IncomingLag = IncomingLag;
            settings.IncomingLossPercentage = IncomingPacketLoss;

            settings.OutgoingJitter = OutgoingJitter;
            settings.OutgoingLag = OutgoinLag;
            settings.OutgoingLossPercentage = OutgoingPacketLoss;
        }
	}
}
