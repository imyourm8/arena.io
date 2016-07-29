using UnityEngine;
using System.Collections;

public class Skill : MonoBehaviour 
{
    private long lastCastTime_ = 0;

    public long Cooldown
    { get; set; }

    public bool Cast()
    {
        if (GameApp.Instance.TimeMs() - lastCastTime_ < Cooldown)
        {
            return false;
        }

        return true;
    }

    protected virtual void OnCast()
    {}
}
