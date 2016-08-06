using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections.Generic;

using SimpleJSON;

public class ExporterForServer : EditorWindow 
{
    [MenuItem("Tools/Exporter To server")]
    private static void ShowExporter()
    {
        EditorWindow.GetWindow(typeof(ExporterForServer));
    }

    private HashSet<Weapon> weapons_ = new HashSet<Weapon>();
    private HashSet<Bullet> bullets_ = new HashSet<Bullet>();
    private HashSet<Player> players_ = new HashSet<Player>();
    private HashSet<Mob> mobs_ = new HashSet<Mob>();
    private HashSet<ExpBlock> expBlocks_ = new HashSet<ExpBlock>();

    private GameObject selectedPrefab_;
    private Vector2 scrollPos_;
    private int tabIndex_ = 0;
    private string[] tabsTitles_ = new string[]
    {
        "Weapons",
        "Bullets",
        "Players",
        "Mobs",
        "ExpBlocks"
    };
        
    void OnGUI()
    {
        tabIndex_ = GUILayout.Toolbar (tabIndex_, tabsTitles_);

        switch(tabIndex_)
        {
            case 0:
                DrawWeapons();
                break;
            case 1:
                DrawBullets();
                break;
            case 2:
                DrawPlayers();
                break;
            case 3:
                DrawMobs();
                break;
            case 4:
                DrawExpBlocks();
                break;
        }
    }

    #region Draw Bullets
    void DrawBullets()
    {
        EditorGUILayout.BeginVertical ();
        selectedPrefab_ = (GameObject)EditorGUILayout.ObjectField(selectedPrefab_, typeof(GameObject), false);
        if (GUILayout.Button("add bullet prefab"))
        {
            var script = selectedPrefab_.GetComponent<Bullet>();
            if (script != null)
                bullets_.Add(script);
        }

        scrollPos_ = EditorGUILayout.BeginScrollView (scrollPos_, GUILayout.MaxHeight (600));

        Bullet toDelete = null;
        foreach(var w in bullets_)
        {
            if (GUILayout.Button(w.gameObject.name))
            {
                toDelete = w;
            }
        }

        if (toDelete != null)
        {
            bullets_.Remove(toDelete);
        }

        EditorGUILayout.EndScrollView ();

        if (GUILayout.Button("Export"))
        {
            JSONArray data = new SimpleJSON.JSONArray();

            foreach(var b in bullets_)
            {
                var node = new SimpleJSON.JSONClass();
                var scale = Mathf.Max(b.transform.localScale.x, b.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["radius"] = new SimpleJSON.JSONData(b.Radius*scale);
                node["type"] = new SimpleJSON.JSONData(b.Type.ToString());
                node["penetrate"] = new SimpleJSON.JSONData(b.Penetrate);
                node["max_dist"] = new SimpleJSON.JSONData(b.MaxDistance);
                node["speed"] = new SimpleJSON.JSONData(b.MoveSpeed);
                data.Add(node);
            }

            File.WriteAllText("server/deploy/arena.io.server/game_data/bullets_export.json", data.ToString ());
        }

        EditorGUILayout.EndVertical ();
    }
    #endregion

    #region Draw Weapons
    void DrawWeapons()
    {
        EditorGUILayout.BeginVertical ();
        selectedPrefab_ = (GameObject)EditorGUILayout.ObjectField(selectedPrefab_, typeof(GameObject), false);
        if (GUILayout.Button("add weapon prefab"))
        {
            var script = selectedPrefab_.GetComponent<Weapon>();
            if (script != null)
                weapons_.Add(script);
        }

        scrollPos_ = EditorGUILayout.BeginScrollView (scrollPos_, GUILayout.MaxHeight (600));

        Weapon toDelete = null;
        foreach(var w in weapons_)
        {
            if (GUILayout.Button(w.gameObject.name))
            {
                toDelete = w;
            }
        }

        if (toDelete != null)
        {
            weapons_.Remove(toDelete);
        }

        EditorGUILayout.EndScrollView ();

        if (GUILayout.Button("Export"))
        {
            JSONArray data = new SimpleJSON.JSONArray();

            foreach(var w in weapons_)
            {
                var node = new SimpleJSON.JSONClass();
                var spawnPoints = new SimpleJSON.JSONArray();
                node["spawnPoints"] = spawnPoints;
                foreach(var spawnPoint in w.GetSpawnPoints())
                {
                    var p = new SimpleJSON.JSONClass();
                    p["x"] = new SimpleJSON.JSONData(spawnPoint.transform.localPosition.x);
                    p["y"] = new SimpleJSON.JSONData(spawnPoint.transform.localPosition.y);
                    p["rot"] = new SimpleJSON.JSONData(spawnPoint.transform.localRotation.eulerAngles.z);
                    p["bullet"] = new SimpleJSON.JSONData(spawnPoint.Bullet.ToString());
                    spawnPoints.Add(p);
                }
                node["sp"] = spawnPoints;
                node["type"] = w.GetWeaponType().ToString();
                data.Add(node);
            }

            File.WriteAllText("server/deploy/arena.io.server/game_data/weapon_export.json", data.ToString ());
        }

        EditorGUILayout.EndVertical ();
    }

    #endregion

    #region Draw Players
    void DrawPlayers()
    {
        EditorGUILayout.BeginVertical ();
        selectedPrefab_ = (GameObject)EditorGUILayout.ObjectField(selectedPrefab_, typeof(GameObject), false);
        if (GUILayout.Button("add player prefab"))
        {
            var script = selectedPrefab_.GetComponent<Player>();
            if (script != null)
                players_.Add(script);
        }

        scrollPos_ = EditorGUILayout.BeginScrollView (scrollPos_, GUILayout.MaxHeight (600));

        Player toDelete = null;
        foreach(var w in players_)
        {
            if (GUILayout.Button(w.gameObject.name))
            {
                toDelete = w;
            }
        }

        if (toDelete != null)
        {
            players_.Remove(toDelete);
        }

        EditorGUILayout.EndScrollView ();

        if (GUILayout.Button("Export"))
        {
            JSONArray data = new SimpleJSON.JSONArray();

            foreach(var w in players_)
            {
                var node = new SimpleJSON.JSONClass();
                var scale = Mathf.Max(w.transform.localScale.x, w.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["weapon"] = new SimpleJSON.JSONData(w.WeaponUsed.ToString());
                node["radius"] = new SimpleJSON.JSONData((w.Collider as CircleCollider2D).radius*scale);
                node["class"] = new SimpleJSON.JSONData(w.Class.ToString());
                data.Add(node);
            }

            File.WriteAllText("server/deploy/arena.io.server/game_data/players_export.json", data.ToString ());
        }

        EditorGUILayout.EndVertical ();
    }

    #endregion

    #region Draw Mobs
    void DrawMobs()
    {
        EditorGUILayout.BeginVertical ();
        selectedPrefab_ = (GameObject)EditorGUILayout.ObjectField(selectedPrefab_, typeof(GameObject), false);
        if (GUILayout.Button("add mob prefab"))
        {
            var script = selectedPrefab_.GetComponent<Mob>();
            if (script != null)
                mobs_.Add(script);
        }

        scrollPos_ = EditorGUILayout.BeginScrollView (scrollPos_, GUILayout.MaxHeight (600));

        Mob toDelete = null;
        foreach(var w in mobs_)
        {
            if (GUILayout.Button(w.gameObject.name))
            {
                toDelete = w;
            }
        }

        if (toDelete != null)
        {
            mobs_.Remove(toDelete);
        }

        EditorGUILayout.EndScrollView ();

        if (GUILayout.Button("Export"))
        {
            JSONArray data = new SimpleJSON.JSONArray();

            foreach(var w in mobs_)
            {
                var node = new SimpleJSON.JSONClass();
                var scale = Mathf.Max(w.transform.localScale.x, w.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["radius"] = new SimpleJSON.JSONData((w.Collider as CircleCollider2D).radius*scale);
                node["type"] = new SimpleJSON.JSONData(w.Type.ToString());
                data.Add(node);
            }

            File.WriteAllText("server/deploy/arena.io.server/game_data/mobs_export.json", data.ToString ());
        }

        EditorGUILayout.EndVertical ();
    }

    #endregion

    #region Draw Exp Blocks
    void DrawExpBlocks()
    {
        EditorGUILayout.BeginVertical ();
        selectedPrefab_ = (GameObject)EditorGUILayout.ObjectField(selectedPrefab_, typeof(GameObject), false);
        if (GUILayout.Button("add exp prefab"))
        {
            var script = selectedPrefab_.GetComponent<ExpBlock>();
            if (script != null)
                expBlocks_.Add(script);
        }

        scrollPos_ = EditorGUILayout.BeginScrollView (scrollPos_, GUILayout.MaxHeight (600));

        ExpBlock toDelete = null;
        foreach(var w in expBlocks_)
        {
            if (GUILayout.Button(w.gameObject.name))
            {
                toDelete = w;
            }
        }

        if (toDelete != null)
        {
            expBlocks_.Remove(toDelete);
        }

        EditorGUILayout.EndScrollView ();

        if (GUILayout.Button("Export"))
        {
            JSONArray data = new SimpleJSON.JSONArray();

            foreach(var w in expBlocks_)
            {
                var node = new SimpleJSON.JSONClass();

                var scale = Mathf.Max(w.transform.localScale.x, w.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["radius"] = new SimpleJSON.JSONData((w.Collider as CircleCollider2D).radius * scale);
                node["type"] = new SimpleJSON.JSONData(w.BlockType.ToString());
                data.Add(node);
            }

            File.WriteAllText("server/deploy/arena.io.server/game_data/exp_blocks_export.json", data.ToString ());
        }

        EditorGUILayout.EndVertical ();
    }

    #endregion
}
