using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class UserDataControl : MonoBehaviour 
{
    [SerializeField]
    private Text nickname;

    [SerializeField]
    private Text coins;

    [SerializeField]
    private Text profileLevel;

    void Update()
    {
        nickname.text = User.Instance.Name;
        profileLevel.text = User.Instance.Level.ToString();
        coins.text = User.Instance.Coins.ToString();
    }
}
