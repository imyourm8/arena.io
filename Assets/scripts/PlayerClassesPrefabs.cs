using UnityEngine;
using System.Collections;

public class PlayerClassesPrefabs : SingletonMonobehaviour<PlayerClassesPrefabs> 
{
    [SerializeField]
    private PlayerClassesDict prefabs = null;

    void Awake()
    {
        instance_ = this;
    }

    public Player GetPlayerClass(proto_profile.PlayerClasses @class)
    {
        return prefabs[@class].GetPooled().GetComponent<Player>();
    }
}
