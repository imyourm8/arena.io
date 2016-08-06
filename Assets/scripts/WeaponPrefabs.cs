using UnityEngine;
using System.Collections;

public class WeaponPrefabs : SingletonMonobehaviour<WeaponPrefabs>
{
    [SerializeField]
    private WeaponDict prefabs;

	void Awake () 
    {
	    instance_ = this;
	}

    public GameObject Get(proto_game.Weapons wep)
    {
        return prefabs[wep].GetPooled();
    }
}
