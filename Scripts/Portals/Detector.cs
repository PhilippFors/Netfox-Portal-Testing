using System;
using System.Collections.Generic;
using Godot;

namespace Netfox_Test.Scripts.Portals;

public partial class TrackedBody : GodotObject
{
    public PhysicsBody3D body;
    public Vector3 lastPosition;
    public Node meshDuplicator;
    public float trackStartTime;

    public override bool Equals(object obj)
    {
        return obj is TrackedBody other &&
               body == other.body;
    }

    protected bool Equals(TrackedBody other)
    {
        return Equals(body, other.body);
    }

    public override int GetHashCode()
    {
        return (body != null ? body.GetHashCode() : 0);
    }
}

[GlobalClass]
public partial class Detector : Node
{
    [Export] public ShapeCast3D shapeCast;
    [Export] private CollisionShape3D environmentFloor;
    [Export] private Portal portal;

    public List<TrackedBody> trackedBodies = new List<TrackedBody>();

    public Action<PhysicsBody3D> onPortalEnter;
    public Action<PhysicsBody3D> onPortalExit;

    public override void _Process(double delta)
    {
        if (!portal.isPlaced)
            return;

        environmentFloor.Disabled = portal.GlobalTransform.Basis.Y.Dot(Vector3.Up) < 1;
        shapeCast.ForceShapecastUpdate();
        var collisionCount = shapeCast.GetCollisionCount();
        var shapeCastColliders = new List<PhysicsBody3D>();
        for (int i = 0; i < collisionCount; i++)
        {
            var body = shapeCast.GetCollider(i);
            if (body is not PhysicsBody3D physicsBody3D)
            {
                continue;
            }

            shapeCastColliders.Add(physicsBody3D);
            AddPhysicsBody(physicsBody3D, false);
        }

        for (int i = 0; i < trackedBodies.Count; i++)
        {
            var body = trackedBodies[i];
            if (!shapeCastColliders.Contains(body.body))
            {
                shapeCastColliders.Remove(body.body);
                RemovePhysicsBody(body.body);
                i--;
            }
        }
    }

    public TrackedBody AddPhysicsBody(PhysicsBody3D body, bool teleportedThisFrame)
    {
        var res = trackedBodies.Find(x => x.body == body);
        if (res != null)
        {
            return res;
        }

        var newBody = new TrackedBody()
        {
            body = body,
            lastPosition = body.GlobalPosition,
            trackStartTime = Time.GetTicksMsec(),
        };

        body.SetCollisionLayerValue(1, false);
        body.SetCollisionMaskValue(1, false);
        body.SetCollisionLayerValue(3, true);
        body.SetCollisionMaskValue(3, true);

        trackedBodies.Add(newBody);
        onPortalEnter?.Invoke(body);
        // GD.Print("Portal Enter: " + body.Name);
        return newBody;
    }

    public void RemovePhysicsBody(PhysicsBody3D body)
    {
        var res = trackedBodies.Find(x => x.body == body);
        if (res == null)
        {
            return;
        }

        body.SetCollisionLayerValue(1, true);
        body.SetCollisionMaskValue(1, true);
        body.SetCollisionLayerValue(3, false);
        body.SetCollisionMaskValue(3, false);
        trackedBodies.Remove(res);
        onPortalExit?.Invoke(body);
        // GD.Print("Portal Exit: " + body.Name);
    }
}