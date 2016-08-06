using UnityEngine;
using System.Collections;

[System.Serializable]
public class MobPrefabDict : SerializableDictionaryBase<proto_game.MobType, GameObject> {}

public class MobPrefabs : SingletonMonobehaviour<MobPrefabs>
{
    [SerializeField]
    private MobPrefabDict prefabs;

	void Awake () 
    {
	    instance_ = this;
	}

    public GameObject Get(proto_game.MobType mob)
    {
        return prefabs[mob].GetPooled();
    }
}
