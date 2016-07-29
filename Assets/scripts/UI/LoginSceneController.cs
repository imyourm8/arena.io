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

public class LoginSceneController : Scene
{
    private enum AuthType
    {
        Nickname,
        FB
    }

    [SerializeField]
    private GameObject nicknamePanel;

    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private Button fbLogin;

    [SerializeField]
    private Text nicknameLoginField;

    [SerializeField]
    private Button nicknameLogin;

    [SerializeField]
    private Button changeNicknameBtn; 

    [SerializeField]
	private Text nickname;

    private AuthType authType_;
    private Events.Slot<string> evtSlot_;

	void Awake () 
    {      
        evtSlot_ = new Events.Slot<string>(Events.GlobalNotifier.Instance);
        evtSlot_.SubscribeOn(GameApp.ConnectToServerSuccess, HandleServerConnection);

        GameApp.Instance.Client.OnServerResponse += HandleResponse;
        GameApp.Instance.Client.OnStatusChange += HandleOnStatusChange;

        Prepare();
	}

    private void HandleServerConnection(Events.IEvent<string> evt)
    {
        if (!FB.IsInitialized) 
        {
            // Initialize the Facebook SDK
            FB.Init("559037884173991", onInitComplete: InitCallback);
        } else 
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    private void Prepare()
    {
        loginPanel.SetActive(true);
        //nicknamePanel.SetActive(false);
        fbLogin.interactable = FB.IsInitialized;
        //changeNicknameBtn.interactable = true;
    }

    private void InitCallback ()
    {
        if (FB.IsInitialized) 
        {
            FB.ActivateApp();
            fbLogin.interactable = true;
        } else 
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    public void LoginWithNickname()
    {
        authType_ = AuthType.Nickname;
        GameApp.Instance.Client.Connect();
    }

    public void LoginWithFB()
    {
        authType_ = AuthType.FB;
        GameApp.Instance.Client.Connect();
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn) 
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            TryLoginWithFB();
        } else
        {
            Debug.Log("User cancelled login");
        }
    }

    void TryLoginWithNickname () 
    {
        if (GameApp.Instance.Client.Status == ExitGames.Client.Photon.StatusCode.Connect)
        {
            AuthWithNickname();
        }
        else 
        {
            Debug.LogError("No connection to server!");
        }
    }

	void TryLoginWithFB () 
    {
        if (GameApp.Instance.Client.Status == ExitGames.Client.Photon.StatusCode.Connect)
        {
            AuthWithFB();
        }
        else 
        {
			Debug.LogError("No connection to server!");
		}
	}

	void HandleOnStatusChange (ExitGames.Client.Photon.StatusCode status)
	{
		if (status == ExitGames.Client.Photon.StatusCode.Connect) 
        {
			switch(authType_)
            {
                case AuthType.Nickname:
                    TryLoginWithNickname();
                    break;
                case AuthType.FB:
                    FB.LogInWithReadPermissions (
                        new List<string>() {"public_profile", "email", "user_friends"},
                        AuthCallback
                    );
                    break;
            }
		}
        else if (status == ExitGames.Client.Photon.StatusCode.Disconnect)
        {
            SceneManager.Instance.SetActive(SceneManager.Scenes.Login);
        }
	}

    private void AuthWithNickname()
    {
        proto_auth.AdminAuth.Request authReq = new proto_auth.AdminAuth.Request ();
        authReq.name = nicknameLoginField.text;

        GameApp.Instance.Client.Send(authReq, proto_common.Commands.ADMIN_AUTH);
    }

	private void AuthWithFB () 
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
        else if (response.type == Commands.ADMIN_AUTH)
        {
            HandleAdminAuth(response);
        }
	}

    private void HandleNicknameChanged(proto_common.Response response)
    {
        var nickRes = response.Extract<proto_profile.ChangeNickname.Response>(Commands.CHANGE_NICKNAME);
        if (nickRes.success)
        {
            ShowHeroSelection();
        }
    }

    private void HandleAdminAuth(proto_common.Response response) 
    {
        var authRes = 
            ProtoBuf.Extensible.GetValue<proto_auth.AdminAuth.Response> (response, (int)proto_common.Commands.ADMIN_AUTH);

        LoadUser(authRes.info);
    }

    private void HandleAuth(proto_common.Response response) 
    {
		var authRes = 
			ProtoBuf.Extensible.GetValue<proto_auth.Auth.Response> (response, (int)proto_common.Commands.AUTH);

        LoadUser(authRes.info);
	}

    private void LoadUser(proto_profile.UserInfo info)
    {
        User.Instance.Coins = info.coins;
        User.Instance.Classes = info.classesInfo;
        User.Instance.Name = info.name;
        User.Instance.Level = info.level;

        if (info.name == "" || info.name == null)
        {
            //create nickname first
            //nicknamePanel.SetActive(true);

            //nicknameLogin.onClick.AddListener (OnNicknameChange);
        }
        else 
        {
            ShowHeroSelection();
        }
    }

    void OnNicknameChange()
    {
        /*
        nicknameLogin.interactable = false;

        var req = new proto_profile.ChangeNickname.Request();
        req.name = nickname.text;
        User.Instance.Name = nickname.text;
        GameApp.Instance.Client.Send(req, Commands.CHANGE_NICKNAME);
        */
    }

    void ShowHeroSelection()
    {
        SceneManager.Instance.SetActive(SceneManager.Scenes.SelectHero);
    }
}
