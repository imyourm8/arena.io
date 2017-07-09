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

    [SerializeField]
    private long movementInterpolationTime_ = 200;

    [SerializeField]
    private float extrapolateNetworkedEntitieTime = 0.25f;

    [SerializeField]
    private int movementUpdateFrequency = 5;
    private RequestsManager requestsManager_ = new RequestsManager();
    private ServerTimeSync timeSync_ = new ServerTimeSync();
    private long adaptiveInterpolationTime_ = 0;

    public float ExtrapolateNetworkedEntitiesTime
    {
        get { return extrapolateNetworkedEntitieTime; }
    }

	void Start () 
    {
        Application.runInBackground = true;
        string ip = "127.0.0.1:4530";
        //string ip = "127.0.0.1:4530";
        //string ip = "46.188.22.12:4530";
		client_ = new ServerClient (ip, ExitGames.Client.Photon.ConnectionProtocol.Tcp);
		client_.OnStatusChange += HandleOnStatusChange;

        DOTween.SetTweensCapacity(3000, 1000);

        client_.OnServerResponse += (proto_common.Response response) => requestsManager_.Update(response);

        SceneManager.Instance.SetActive(SceneManager.Scenes.Login);
        /*
        float p = 49.88312f;
        var bytes = BitConverter.GetBytes(p);

        if (BitConverter.IsLittleEndian)
        {
            var sign = bytes[3] & 1;
            var exp =  ((bytes[2] & 1) << 8) | (bytes[3] & (254));
            var mantisa = (bytes[0] << 16) | (bytes[1]<<8) | (bytes[2] & 254);

            Debug.LogFormat("Sign: {0} Exp: {1} Mantissa: {2}", sign,exp,mantisa);
        }
        */
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
            timeSync_.Update();
		    client_.Service();

            adaptiveInterpolationTime_ = movementInterpolationTime_;// + timeSync_.Ping;
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
   
    public long Latency
    {
        get { return timeSync_.Latency; }
    }

	public long ServerTimeMs()
	{
        return ClientTimeMs() + timeSync_.ServerTimeDifference;
        //return client_.ServerTime;
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

    public long MovementInterpolationTime
    {
        get { return adaptiveInterpolationTime_; } 
    }

    public float MovementUpdateDT
    {
        get { return 1.0f / (float)movementUpdateFrequency; } 
    }
}
