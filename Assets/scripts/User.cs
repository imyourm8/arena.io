using UnityEngine;
using System.Collections.Generic;

public class User : TapCommon.Singleton<User> 
{
    public int Coins { get; set; }
    public string Name { get; set; }
    public List<proto_profile.ClassInfo> Classes { get; set; }
}
