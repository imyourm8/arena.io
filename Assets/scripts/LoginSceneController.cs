using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using proto_auth;
using proto_common;
using ProtoBuf;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Facebook.Unity;

public class LoginSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject nicknamePanel;

	public Button okBtn;
	public Text nickname;
    public GameObject arena_;

	void Start () 
    {
		Debug.Log ("Started login scene");      

        if (!FB.IsInitialized) {
            // Initialize the Facebook SDK
            FB.Init("559037884173991", onInitComplete: InitCallback);
        } else {
            // Already initialized, signal an app activation App Event
            //FB.ActivateApp();
        }
	}

    private void Prepare()
    {
        okBtn.interactable = true;
    }

    private void InitCallback ()
    {
        if (FB.IsInitialized) {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            FB.LogInWithReadPermissions (
                new List<string>() {"public_profile", "email", "user_friends"},
                AuthCallback
            );
        } else {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn) 
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            TryLogin();
        } else
        {
            Debug.Log("User cancelled login");
        }
    }

	void TryLogin () 
    {
        GameApp.Instance.Client.OnServerResponse += HandleResponse;
        GameApp.Instance.Client.OnStatusChange += HandleOnStatusChange;
		Debug.Log ("Login clicked");

        if (GameApp.Instance.Client.Status == ExitGames.Client.Photon.StatusCode.Connect)
        {
            Auth();
        }
        else if (!GameApp.Instance.Client.Connect ()) 
        {
			Debug.LogError("No connection to Internet");
		}
	}

	void HandleOnStatusChange (ExitGames.Client.Photon.StatusCode status)
	{
		if (status == ExitGames.Client.Photon.StatusCode.Connect) 
        {
			Auth();
		}
        else if (status == ExitGames.Client.Photon.StatusCode.Disconnect)
        {
            HideArena();
        }
	}

	private void Auth () 
    {
		proto_auth.Auth.Request authReq = new proto_auth.Auth.Request ();
		proto_auth.Auth.Request.OAuth oauth = new proto_auth.Auth.Request.OAuth ();
		oauth.provider = proto_auth.Auth.OAuthProvider.FB;
        oauth.access_token = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString;
        oauth.uid = Facebook.Unity.AccessToken.CurrentAccessToken.UserId;

		authReq.m_oauth = oauth;
		authReq.type = proto_auth.Auth.AuthType.OAuth;

		proto_common.Request req = new proto_common.Request ();
		req.type = proto_common.Commands.AUTH;
		req.id = 0;

		ProtoBuf.Extensible.AppendValue (req, (int)proto_common.Commands.AUTH, authReq);
        GameApp.Instance.Client.Send (req);
	}

	private void HandleResponse(proto_common.Response response) 
    {
		if (response.type == proto_common.Commands.AUTH) 
        {
			HandleAuth(response);
		}
        else if (response.type == Commands.CHANGE_NICKNAME)
        {
            HandleNicknameChanged(response);
        }
	}

    private void HandleNicknameChanged(proto_common.Response response)
    {
        var nickRes = response.Extract<proto_profile.ChangeNickname.Response>(Commands.CHANGE_NICKNAME);
        if (nickRes.success)
        {
            ShowArena();
        }
    }

    private void HandleAuth(proto_common.Response response) 
    {
		var authRes = 
			ProtoBuf.Extensible.GetValue<proto_auth.Auth.Response> (response, (int)proto_common.Commands.AUTH);
            
        if (authRes.info.name == "" || authRes.info.name == null)
        {
            //create nickname first
            nicknamePanel.SetActive(true);

            okBtn.onClick.AddListener (OnNicknameChange);
        }
        else 
        {
            User.Instance.Coins = authRes.info.coins;
            User.Instance.Classes = authRes.info.classesInfo;
            User.Instance.Name = authRes.info.name;
            User.Instance.Level = authRes.info.level;
            
            ShowArena();
        }
	}

    void OnNicknameChange()
    {
        okBtn.interactable = false;

        var req = new proto_profile.ChangeNickname.Request();
        req.name = nickname.text;

        GameApp.Instance.Client.Send(req, Commands.CHANGE_NICKNAME);
    }

    void ShowArena()
    {
        SwitchArenaView(true);
    }

    void HideArena()
    {
        SwitchArenaView(false);
    }

    public void SwitchArenaView(bool show)
    {
        arena_.SetActive(show);
        gameObject.SetActive(!show);
    }

	void onDestroy() 
    {
        GameApp.Instance.Client.OnServerResponse -= HandleResponse;
        GameApp.Instance.Client.OnStatusChange -= HandleOnStatusChange;
	}
}
