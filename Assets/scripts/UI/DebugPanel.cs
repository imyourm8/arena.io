using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class DebugPanel : MonoBehaviour 
{
    [SerializeField]
    private Text latencyValue = null;

    [SerializeField]
    private Text networkBytesPerSecondValu = null;

    [SerializeField]
    private Text networkBytesTotalValue = null;

	void Update () 
    {
        latencyValue.text = GameApp.Instance.Latency.ToString();
        networkBytesTotalValue.text = (GameApp.Instance.Client.TotalBytesReceived).ToString();
	}
}
