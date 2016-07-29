using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

public class SceneManager : SingletonMonobehaviour<SceneManager>
{
    public enum Scenes
    {
        Login,
        Arena,
        FinishGame,
        SelectHero
    }

    [SerializeField]
    private ScenesDict scenes = null;

    [SerializeField]
    private LoadingScreen loading = null;

    private List<Scene> activeScenes_ = new List<Scene>();

    private void Awake()
    {
        instance_ = this;
        loading.Hide();
    }

    private IEnumerator WaitForScene(Scenes sceneId, Action cb)
    {
        var scene = scenes[sceneId];
        loading.Show();

        while (!scene.Ready)
            yield return null;

        loading.Hide();
        if (cb != null)
            cb();
    }

    public void SetActive(Scenes sceneId)
    {
        scenes[sceneId].OnBeforeShow();
        StartCoroutine(WaitForScene(sceneId, ()=>
        {
            foreach(var scene in activeScenes_)
            {
                HideScene(scene);
            }

            activeScenes_.Clear();

            Add(sceneId);
        }));
    }

    private void Add(Scenes sceneId)
    {
        var scene = scenes[sceneId];
        scene.Show();
        activeScenes_.Add(scene);
    }

    private void HideScene(Scene scene)
    {
        scene.LinkedScenes.Clear();
        scene.OnBeforeHide();
        scene.Hide();
    }

    public void Pop(Scene scene)
    {
        foreach(var linked in scene.LinkedScenes)
        {
            linked.Pop();
        }

        HideScene(scene);

        var found = activeScenes_.IndexOf(scene);
        if (found > -1)
        {
            activeScenes_.RemoveAt(found);
        }
    }

    public void AddLinked(Scenes sceneId, Scenes linkTo)
    {
        if (sceneId != linkTo)
        {
            scenes[sceneId].OnBeforeShow();
            StartCoroutine(WaitForScene(sceneId, ()=>
            {
                var scene = scenes[sceneId];
                var linkToScene = scenes[linkTo];

                linkToScene.LinkedScenes.Add(scene);
                scene.Show();
            }));
        }
    }
}
