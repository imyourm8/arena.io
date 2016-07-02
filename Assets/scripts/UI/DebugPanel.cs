using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class DebugPanel : MonoBehaviour 
{
    [SerializeField]
    private Text latencyValue;

    [SerializeField]
    private Text networkBytesPerSecondValue;

    [SerializeField]
    private Text networkBytesTotalValue;

	void Update () 
    {
        latencyValue.text = GameApp.Instance.Latency.ToString();
        networkBytesPerSecondValue.text = ((float)GameApp.Instance.BytesReceivedPerSecond / 1024.0f).ToString();
        networkBytesTotalValue.text = ((float)GameApp.Instance.BytesReceivedTotal/1024.0f).ToString();
	}
}
