using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatPoint : MonoBehaviour 
{
    [SerializeField]
    private Image image;

    public void SwitchColor(Color color)
    {
        image.color = color;
    }
}
