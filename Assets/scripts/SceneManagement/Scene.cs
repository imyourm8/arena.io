using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Scene : MonoBehaviour 
{
    public bool Ready
    {
        get; protected set;    
    }

    public Scene()
    {
        Ready = true;
        LinkedScenes = new List<Scene>();
    }

    public List<Scene> LinkedScenes
    { get; set; }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Pop()
    {
        SceneManager.Instance.Pop(this);
    }

    public virtual void OnBeforeShow()
    {}

    public virtual void OnAfterShow()
    {}

    public virtual void OnBeforeHide()
    {}
}
