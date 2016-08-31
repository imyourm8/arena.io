using UnityEngine;
using System.Collections.Generic;

public class NetworkProjectileManager
{
    private HashSet<Bullet> projectiles_ = new HashSet<Bullet>();

    public Player Player
    { get; set; }

    public void Register(Bullet projectile)
    {
        projectiles_.Add(projectile);
    }

    public void Unregister(Bullet projectile)
    {
        projectiles_.Remove(projectile);
    }

    public void Clear()
    {
        projectiles_.Clear();
    }

    public void CorrectProjectilesRelativelyToLocalPlayer()
    {
        return;
        var playerPosition = Player.Position;
        var serverPosition = Player.LastServerPosition;
        //var diff = playerPosition - serverPosition;
        //var distanceTraveledPerUpdate = Player.Stats.GetFinValue(proto_game.Stats.MovementSpeed);
        //distanceTraveledPerUpdate *= (GameApp.Instance.MovementUpdateDT + (float)GameApp.Instance.Latency/1000.0f);

        foreach(var projectile in projectiles_)
        {
            /*
            var projectionPoint = GetProjectionPointOnLine(projectile.StartPoint, projectile.ServerPosition, playerPosition);

            float distance = Vector2.Distance(projectionPoint, playerPosition);

            if (distance <= distanceTraveledPerUpdate)
            {
                distance = distance > distanceTraveledPerUpdate ? distanceTraveledPerUpdate : distance;
                var correctedPosition = playerPosition - projectionPoint;
                correctedPosition.Normalize();
                //correct direction of projectile
                correctedPosition = correctedPosition * distance;
                correctedPosition += projectile.ServerPosition;
                //correctedPosition -= projectile.ServerPosition;
                correctedPosition.Normalize();
                projectile.SetDirection(correctedPosition);
            }
            */
            //projectile.Position = projectile.ServerPosition + diff;
            //projectile.MovementAdjustment = diff;
            //Debug.LogWarningFormat("Proj {0} pos {1} {2}", projectile.ID, /*projectile.Position.x, projectile.Position.y,*/ diff.x, diff.y);
            var serverRelativePosition = projectile.ServerPosition - serverPosition;
            var currentProjectilePosition = playerPosition + serverRelativePosition;

            /*if (diff.magnitude > distanceTraveledPerUpdate)
            {
                diff = diff.normalized * distanceTraveledPerUpdate;
            }*/
            projectile.MovementAdjustment = currentProjectilePosition - projectile.ServerPosition;
        }
    }

    private float DistanceBetweenLineAndPoint(Vector2 v, Vector2 w, Vector2 p)
    {
        Vector2 projection = GetProjectionPointOnLine(v, w, p);
        return Vector2.Distance(projection, p);
    }

    private Vector2 GetProjectionPointOnLine(Vector2 v, Vector2 w, Vector2 p)
    {
        var line = v - w;
        float l2 = line.sqrMagnitude;
        if (Mathf.Approximately(l2, 0.0f))
        {
            return v;
        }
        float t = Mathf.Max(0.0f, Mathf.Min(1, Vector2.Dot(p - v, w - v) / l2));
        return v + t * (w - v);
    }
}
