using UnityEngine;
using System.Collections;

public struct PhysicalState
{
    public Vector2 Position
    { get; set; }

    public float Rotation
    { get; set; }

    public Vector2 Velocity
    { get; set; }

    public Vector2 RecoilVelocity
    { get; set; }

    public float Recoil
    { get; set; }
}
