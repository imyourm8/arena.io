using UnityEngine;
using System.Collections;

public class SkillBullet : Bullet 
{
    protected override void OnInit()
    {
        penetrate = true;
    }
}
