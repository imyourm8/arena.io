using UnityEngine;
using System.Collections;

public class BulletSpawnPoint : MonoBehaviour 
{
    [SerializeField]
    private proto_game.Bullets bullet;

    public proto_game.Bullets Bullet
    {
        get { return bullet; } 
    }
}
