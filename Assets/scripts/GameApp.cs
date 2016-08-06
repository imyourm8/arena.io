using UnityEngine;
using System;
using System.Collections;
using Facebook.Unity;
using DG.Tweening;

public class GameApp : SingletonMonobehaviour<GameApp> 
{
    public static string ConnectToServerSuccess = "connect_to_srv_evt";

    [SerializeField]
    private LoginSceneController loginUI;

	private readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	private proto_profile.UserInfo userInfo_;
	private ServerClient client_;
	private ServerTimeSync timeSync_;
    private long nextBytesReceivedUpdate_ = 0;
    private long bytesReceivedPerSecond_ = 0;
    private long bytesReceivedTotal_ = 0;

    [SerializeField]
    private float movementInterpolationTime_ = 0.2f;

    [SerializeField]
    private int movementUpdateFrequency = 5;
    private RequestsManager requestsManager_ = new RequestsManager();

	void Start () 
    {
        //string ip = "192.168.1.2:4530";
        string ip = "127.0.0.1:4530";
        //string ip = "46.188.22.12:4530";
		client_ = new ServerClient (ip, ExitGames.Client.Photon.ConnectionProtocol.Tcp);
		client_.OnStatusChange += HandleOnStatusChange;
		timeSync_ = new ServerTimeSync ();

        nextBytesReceivedUpdate_ = ClientTimeMs() + 1000;
        DOTween.SetTweensCapacity(3000, 1000);

        client_.OnServerResponse += (proto_common.Response response) => requestsManager_.Update(response);

        SceneManager.Instance.SetActive(SceneManager.Scenes.Login);
	}

	void HandleOnStatusChange (ExitGames.Client.Photon.StatusCode status)
	{
		if (status == ExitGames.Client.Photon.StatusCode.Connect)
		{
			timeSync_.Start();
            Events.GlobalNotifier.Instance.Trigger(ConnectToServerSuccess);
		}
	}

	void Update () 
    {
		if (client_ != null) 
        {
		    client_.Service();
			timeSync_.Update();
		}

        if (ClientTimeMs() >= nextBytesReceivedUpdate_)
        {
            nextBytesReceivedUpdate_ += 1000;
            bytesReceivedPerSecond_ = client_.TotalBytesReceived - bytesReceivedTotal_;
            bytesReceivedTotal_ = client_.TotalBytesReceived;
        }
	}

	void Awake() 
    {
		if(Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this);
		}
		else
		{
			if(this != Instance)
				Destroy(this.gameObject);
		}
	}

    public RequestsManager RequestsManager
    { get { return requestsManager_; }}

	public long ClientTimeMs()
	{
		return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
	}

	public long Ping()
	{
		return timeSync_.Ping;
	}

    public long Latency
    {
        get { return timeSync_.Latency; }
    }

	public long TimeMs()
	{
		return ClientTimeMs() + timeSync_.ServerTimeDifference;
	}

    public long BytesReceivedPerSecond
    {
        get { return bytesReceivedPerSecond_; }
    }

    public long BytesReceivedTotal
    {
        get { return bytesReceivedTotal_; }
    }

	public ServerClient Client 
    {
		get { return client_; }
	}

	public proto_profile.UserInfo UserInfo 
    {
		get { return userInfo_; }
		set { userInfo_ = value; }
	}

    public float MovementInterpolationTime
    {
        get { return movementInterpolationTime_; } 
    }

    public float MovementUpdateDT
    {
        get { return 1.0f / (float)movementUpdateFrequency; } 
    }
}
