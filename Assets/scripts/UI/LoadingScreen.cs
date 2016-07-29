using UnityEngine;
using System.Collections;

public class LoadingScreen : MonoBehaviour 
{
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
