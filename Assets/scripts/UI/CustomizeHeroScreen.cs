using UnityEngine;
using System.Collections;

public class CustomizeHeroScreen : MonoBehaviour 
{
    [SerializeField]
    private arena.ArenaController arena_ = null;

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
        arena_.gameObject.SetActive(true);
        arena_.OnJoinGame();
    }

    public void Show(bool findNewGame = true)
    {
        gameObject.SetActive(true);

        if (findNewGame)
        {
            GameApp.Instance.Client.OnServerResponse += HandleServerResponse;

            var findRequest = new proto_game.FindRoom();
            GameApp.Instance.Client.Send(findRequest, proto_common.Commands.FIND_ROOM);
        } 
        else 
        {
            ShowClassSelection();
        }
    }

    void ShowClassSelection()
    {
        findRoomMsg.SetActive(false);
        classSelection.Refresh();
        classSelection.Show();
    }

    void HandleServerResponse(proto_common.Response response)
    {
        if (response.type == proto_common.Commands.FIND_ROOM)
        {
            HandleFindRoom(response);
        }
    }

    private void HandleFindRoom(proto_common.Response response)
    {
        //show selection screen
        ShowClassSelection();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        classSelection.Hide();
        findRoomMsg.SetActive(true);

        GameApp.Instance.Client.OnServerResponse -= HandleServerResponse;
    }
}
