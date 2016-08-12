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
    private HashSet<PowerUp> powerUps_ = new HashSet<PowerUp>();
    private HashSet<PickUp> pickUps_ = new HashSet<PickUp>();
    private HashSet<Skill> skills_ = new HashSet<Skill>();

    private GameObject selectedPrefab_;
    private Vector2 scrollPos_;
    private int tabIndex_ = 0;
    private string[] tabsTitles_ = new string[]
    {
        "Weapons",
        "Bullets",
        "Players",
        "Mobs",
        "ExpBlocks",
        "PowerUps",
        "PickUps",
        "Skills"
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
            case 5:
                DrawPowerUps();
                break;
            case 6:
                DrawPickUps();
                break;
            case 7:
                DrawSkills();
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
            JSONArray data = new JSONArray();

            foreach(var b in bullets_)
            {
                var node = new JSONClass();
                var scale = Mathf.Max(b.transform.localScale.x, b.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["radius"] = new JSONData(b.Radius*scale);
                node["type"] = new JSONData(b.Type.ToString());
                node["penetrate"] = new JSONData(b.Penetrate);
                node["time_alive"] = new JSONData(b.TimeAlive);
                node["speed"] = new JSONData(b.MoveSpeed);
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
            JSONArray data = new JSONArray();

            foreach(var w in weapons_)
            {
                var node = new JSONClass();
                var spawnPoints = new JSONArray();
                node["spawnPoints"] = spawnPoints;
                foreach(var spawnPoint in w.GetSpawnPoints())
                {
                    var p = new JSONClass();
                    p["x"] = new JSONData(spawnPoint.transform.localPosition.x);
                    p["y"] = new JSONData(spawnPoint.transform.localPosition.y);
                    p["rot"] = new JSONData(spawnPoint.transform.localRotation.eulerAngles.z);
                    p["bullet"] = new JSONData(spawnPoint.Bullet.ToString());
                    spawnPoints.Add(p);
                }
                node["recoil"] = new JSONData(w.Recoil);
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
            JSONArray data = new JSONArray();

            foreach(var w in players_)
            {
                var node = new JSONClass();
                var scale = Mathf.Max(w.transform.localScale.x, w.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["weapon"] = new JSONData(w.WeaponUsed.ToString());
                node["radius"] = new JSONData((w.Collider as CircleCollider2D).radius*scale);
                node["class"] = new JSONData(w.Class.ToString());
                node["linear_dumping"] = new JSONData(w.Body.drag);
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
            JSONArray data = new JSONArray();

            foreach(var w in mobs_)
            {
                var node = new JSONClass();
                var scale = Mathf.Max(w.transform.localScale.x, w.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["radius"] = new JSONData((w.Collider as CircleCollider2D).radius*scale);
                node["type"] = new JSONData(w.Type.ToString());
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
            JSONArray data = new JSONArray();

            foreach(var w in expBlocks_)
            {
                var node = new JSONClass();

                var scale = Mathf.Max(w.transform.localScale.x, w.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["radius"] = new JSONData((w.Collider as CircleCollider2D).radius * scale);
                node["type"] = new JSONData(w.BlockType.ToString());
                data.Add(node);
            }

            File.WriteAllText("server/deploy/arena.io.server/game_data/exp_blocks_export.json", data.ToString ());
        }

        EditorGUILayout.EndVertical ();
    }

    #endregion

    #region Draw Power Ups Blocks
    void DrawPowerUps()
    {
        EditorGUILayout.BeginVertical ();
        selectedPrefab_ = (GameObject)EditorGUILayout.ObjectField(selectedPrefab_, typeof(GameObject), false);
        if (GUILayout.Button("add power up prefab"))
        {
            var script = selectedPrefab_.GetComponent<PowerUp>();
            if (script != null)
                powerUps_.Add(script);
        }

        scrollPos_ = EditorGUILayout.BeginScrollView (scrollPos_, GUILayout.MaxHeight (600));

        PowerUp toDelete = null;
        foreach(var w in powerUps_)
        {
            if (GUILayout.Button(w.gameObject.name))
            {
                toDelete = w;
            }
        }

        if (toDelete != null)
        {
            powerUps_.Remove(toDelete);
        }

        EditorGUILayout.EndScrollView ();

        if (GUILayout.Button("Export"))
        {
            JSONArray data = new JSONArray();

            foreach(var w in powerUps_)
            {
                var node = new JSONClass();
                var scale = Mathf.Max(w.transform.localScale.x, w.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["radius"] = new JSONData((w.Collider as CircleCollider2D).radius * scale);
                node["type"] = new JSONData(w.PowerUpType.ToString());
                data.Add(node);
            }

            File.WriteAllText("server/deploy/arena.io.server/game_data/power_ups_export.json", data.ToString ());
        }

        EditorGUILayout.EndVertical ();
    }

    #endregion

    #region Draw pick ups
    void DrawPickUps()
    {
        EditorGUILayout.BeginVertical ();
        selectedPrefab_ = (GameObject)EditorGUILayout.ObjectField(selectedPrefab_, typeof(GameObject), false);
        if (GUILayout.Button("add pickup prefab"))
        {
            var script = selectedPrefab_.GetComponent<PickUp>();
            if (script != null)
                pickUps_.Add(script);
        }

        scrollPos_ = EditorGUILayout.BeginScrollView (scrollPos_, GUILayout.MaxHeight (600));

        PickUp toDelete = null;
        foreach(var w in pickUps_)
        {
            if (GUILayout.Button(w.gameObject.name))
            {
                toDelete = w;
            }
        }

        if (toDelete != null)
        {
            pickUps_.Remove(toDelete);
        }

        EditorGUILayout.EndScrollView ();

        if (GUILayout.Button("Export"))
        {
            JSONArray data = new JSONArray();

            foreach(var w in pickUps_)
            {
                var node = new JSONClass();
                var scale = Mathf.Max(w.transform.localScale.x, w.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["radius"] = new JSONData((w.Collider as CircleCollider2D).radius * scale);
                node["type"] = new JSONData(w.PickupType.ToString());
                data.Add(node);
            }

            File.WriteAllText("server/deploy/arena.io.server/game_data/pickups_export.json", data.ToString ());
        }

        EditorGUILayout.EndVertical ();
    }

    #endregion

    #region Skills
    void DrawSkills()
    {
        EditorGUILayout.BeginVertical ();
        selectedPrefab_ = (GameObject)EditorGUILayout.ObjectField(selectedPrefab_, typeof(GameObject), false);
        if (GUILayout.Button("add skill prefab"))
        {
            var script = selectedPrefab_.GetComponent<Skill>();
            if (script != null)
                skills_.Add(script);
        }

        scrollPos_ = EditorGUILayout.BeginScrollView (scrollPos_, GUILayout.MaxHeight (600));

        Skill toDelete = null;
        foreach(var w in skills_)
        {
            if (GUILayout.Button(w.gameObject.name))
            {
                toDelete = w;
            }
        }

        if (toDelete != null)
        {
            skills_.Remove(toDelete);
        }

        EditorGUILayout.EndScrollView ();

        if (GUILayout.Button("Export"))
        {
            JSONArray data = new JSONArray();

            foreach(var w in skills_)
            {
                var node = new JSONClass();
                var scale = Mathf.Max(w.transform.localScale.x, w.transform.localScale.y);
                scale = Mathf.Abs(scale);
                node["recoil"] = new JSONData(w.Recoil);
                node["type"] = new JSONData(w.Type.ToString());
                data.Add(node);
            }

            File.WriteAllText("server/deploy/arena.io.server/game_data/skills_export.json", data.ToString ());
        }

        EditorGUILayout.EndVertical ();
    }

    #endregion
}
