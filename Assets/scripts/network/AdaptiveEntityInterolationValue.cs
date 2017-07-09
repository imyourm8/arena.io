using UnityEngine;
using System.Collections;

public class AdaptiveEntityInterolationValue 
{
    private int baselineValue_ = 0;

    public AdaptiveEntityInterolationValue(int baselineValue)
    {
        baselineValue_ = baselineValue;
    }

    public void Update(int latency)
    {
        
    }
}
