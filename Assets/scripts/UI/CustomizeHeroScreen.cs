using UnityEngine;
using System.Collections;

public class CustomizeHeroScreen : MonoBehaviour 
{
    [SerializeField]
    private arena.ArenaController arena_;

    public void JoinGame()
    {
        arena_.gameObject.SetActive(true);
        arena_.OnJoinGame();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
