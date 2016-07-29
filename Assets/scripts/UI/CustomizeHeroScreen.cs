using UnityEngine;
using System.Collections;

public class CustomizeHeroScreen : Scene 
{
    [SerializeField]
    private ClassSelection classSelection = null;

    [SerializeField]
    private GameObject findRoomMsg = null;

    void Start()
    {
        classSelection.Hide();
    }

    public void JoinGame()
    {
        SceneManager.Instance.SetActive(SceneManager.Scenes.Arena);
    }

    public void Show(bool findNewGame = true)
    {
        gameObject.SetActive(true);

        ShowClassSelection();
    }

    void ShowClassSelection()
    {
        findRoomMsg.SetActive(false);
        classSelection.Refresh();
        classSelection.Show();
    }

    public override void OnBeforeHide()
    {
        gameObject.SetActive(false);
        classSelection.Hide();
    }
}
