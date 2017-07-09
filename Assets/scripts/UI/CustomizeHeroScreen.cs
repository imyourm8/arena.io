using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;

public class CustomizeHeroScreen : Scene 
{
    [SerializeField]
    private ClassSelection classSelection = null;

    [SerializeField]
    private GameObject findRoomMsg = null;

    [SerializeField]
    private GameObject classObjectHolder = null;

    [SerializeField]
    private Text coinsValue = null;

    private GameObject currentClass_ = null;

    void Start()
    {
        classSelection.OnClassChange = HandleClassChange;
    }

    public void JoinGame()
    {
        SceneManager.Instance.SetActive(SceneManager.Scenes.Arena);
    }

    public void Show(bool findNewGame = true)
    {
        ShowClassSelection();
    }

    void ShowClassSelection()
    {
        findRoomMsg.SetActive(false);
        classSelection.Refresh();
        classSelection.Show();
    }

    public override void OnAfterShow()
    {
        base.OnAfterShow();
        ShowClassSelection();
        HandleClassChange();
        UpdateCoins();
    }

    private void UpdateCoins()
    {
        coinsValue.text = User.Instance.Coins.ToString();
    }

    public override void OnBeforeHide()
    {
        //classSelection.Hide();
    }

    private void HandleClassChange()
    {
        if (currentClass_ != null)
        {
            currentClass_.ReturnPooled();    
        }
        var classObject = PlayerClassesPrefabs.Instance.GetPlayerClass(User.Instance.ClassSelected);
        classObject.Init(null); 
        classObject.gameObject.transform.localScale = new Vector3(100, 100, 1);
        classObject.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "FrontObjects";
        classObject.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 100;
        classObject.gameObject.Map(((GameObject wep) => 
        {
            foreach(var r in wep.GetComponentsInChildren<SpriteRenderer>())
            {
                r.sortingLayerName = "FrontObjects";
                r.sortingOrder = 50;
            }
        }));
        classObjectHolder.AddChild(classObject.gameObject);
        currentClass_ = classObject.gameObject;
    }
}
