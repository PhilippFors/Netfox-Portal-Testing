using System;
using System.Collections.Generic;
using Godot;
using Netfox_Test.Scripts.Extensions;
using Netfox;
using Netfox.Logging;
using QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

namespace Netfox_Test.Scripts.Portals;

public partial class Portal : Node3D
{
    [Export] public Portal targetPortal;
    [Export] public Detector detector;
    [Export] public CsgBox3D portalVisual;
    [Export] public bool isPlaced;

    public int cullLayer = 4;
    private float exitScale = 1;
    private List<TrackedBody> bodiesPassedThroughThisTick = new();

    private NetfoxLogger logger = NetfoxLogger.ForNetfox("NetworkRollback");

    public override void _Ready()
    {
        portalVisual.SetLayerMaskValue(1, false);
        portalVisual.SetLayerMaskValue(cullLayer, true);
    }

    public void _rollback_tick(double delta, long tick, bool isFresh)
    {
        foreach (var body in detector.trackedBodies)
        {
            var portalTraveller = body.body.FindChildByType<PortalTraveller>();
            if (portalTraveller == null)
            {
                GD.PrintErr("PortalTraveller not found on body.");
                continue;
            }

            // TODO: Adjust portal side detection. Teleports too late.
            Node3D posNode = portalTraveller.GetPositionNode();
            Vector3 forward = GlobalTransform.Basis.Z;
            var offsetFromPortal = posNode.GlobalPosition - detector.shapeCast.GlobalPosition;
            var prevOffsetFromPortal = body.lastPosition - detector.shapeCast.GlobalPosition;
            var portalSide = offsetFromPortal.Dot(forward);
            var prevPortalSide = prevOffsetFromPortal.Dot(forward);
            if (portalSide <= 0 && prevPortalSide > 0)
            {
                if (targetPortal != null || isPlaced)
                {
                    bodiesPassedThroughThisTick.Add(body);
                }
            }

            body.lastPosition = posNode.GlobalPosition;
        }

        foreach (var body in bodiesPassedThroughThisTick)
        {
            MoveToOtherPortal(body, tick);
        }

        bodiesPassedThroughThisTick.Clear();
    }

    public void SetPortalPosition(Vector3 point)
    {
        SetPosition(point);
        SetProcess(true);
        SetVisible(true);
        isPlaced = true;
    }

    public void SetPortalNormal(Vector3 normal)
    {
        // TODO: Consistent Rotation, including rotating towards where player is looking
        LookAt(GlobalPosition - normal, Vector3.Up);
        portalVisual.SetInstanceShaderParameter("forward", GlobalBasis.Z);
    }

    public void RemovePortal()
    {
        isPlaced = false;
        SetVisible(false);
        SetProcess(false);
    }

    private void MoveToOtherPortal(TrackedBody body, long tick)
    {
        var traveller = body.body.FindChildByType<PortalTraveller>();
        if (traveller == null)
            return;

        var player = (QuantaPlayerController)body.body;
        player.loggerFlag = true;
        player.logTickStart = tick;
        traveller.isTravelling = true;
        traveller.Travel(this, targetPortal);
        logger.LogWarning("Moving " + body.body.GetParent().Name + " through " + Name);
        NetworkRollback.Mutate(traveller, tick);
        
        detector.RemovePhysicsBody(body.body);
        targetPortal.detector.AddPhysicsBody(body.body, true);
    }

    public Transform3D RealToExitTransform(Transform3D real)
    {
        // # Convert from global space to local space at the entrance (this) portal
        Transform3D local = GlobalTransform.AffineInverse() * real;
        // # Compensate for any scale the entrance portal may have
        Transform3D unscaled = local.Scaled(GlobalTransform.Basis.Scale);
        // # Flip it (the portal always flips the view 180 degrees)
        Transform3D flipped = unscaled.Rotated(Vector3.Up, Mathf.Pi);
        // # Apply any scale the exit portal may have (and apply custom exit scale)
        Vector3 exitScaleVector = targetPortal.GlobalTransform.Basis.Scale;
        Transform3D scaledAtExit = flipped.Scaled(Vector3.One / exitScaleVector * exitScale);
        // # Convert from local space at the exit portal to global space
        Transform3D localAtExit = targetPortal.GlobalTransform * scaledAtExit;
        return localAtExit;
    }

    // Return a new position relative to the exit portal based on the real position relative to this portal.
    public Vector3 RealToExitPosition(Vector3 real)
    {
        //Convert from global space to local space at the entrance (this) portal
        var globalTransform = GlobalTransform;
        Vector3 local = globalTransform.AffineInverse() * real;
        //Compensate for any scale the entrance portal may have
        Vector3 unscaled = local * globalTransform.Basis.Scale;
        // # Apply any scale the exit portal may have (and apply custom exit scale)
        Vector3 exitScaleVector = new Vector3(-1, 1, 1) * targetPortal.GlobalTransform.Basis.Scale;
        Vector3 scaledAtExit = unscaled / exitScaleVector * exitScale;
        // # Convert from local space at the exit portal to global space
        Vector3 localAtExit = targetPortal.GlobalTransform * scaledAtExit;
        return localAtExit;
    }

    public Vector3 RealToExitRotation(Vector3 real)
    {
        var globalTransform = GlobalTransform;
        Vector3 local = globalTransform.AffineInverse() * real;
        // # Compensate for any scale the entrance portal may have
        Vector3 unscaled = local * globalTransform.Basis.Scale;
        // # Flip it (the portal always flips the view 180 degrees)
        Vector3 flipped = unscaled.Rotated(Vector3.Up, Mathf.Pi);
        // # Apply any scale the exit portal may have (and apply custom exit scale)
        Vector3 exitScaleVector = targetPortal.GlobalTransform.Basis.Scale;
        Vector3 scaledAtExit = flipped / exitScaleVector * exitScale;
        // # Convert from local space at the exit portal to global space
        Vector3 localAtExit = targetPortal.GlobalTransform * scaledAtExit;
        return localAtExit;
    }

    // ## Return a new direction relative to the exit portal based on the real direction relative to this portal.
    public Vector3 RealToExitDirection(Vector3 real)
    {
        // # Convert from global to local space at the entrance (this) portal
        var globalTransform = GlobalTransform;
        Vector3 local = globalTransform.Basis.Inverse() * real;
        //Compensate for any scale the entrance portal may have
        Vector3 unscaled = local * globalTransform.Basis.Scale;
        //Flip it (the portal always flips the view 180 degrees)
        Vector3 flipped = unscaled.Rotated(Vector3.Up, Mathf.Pi);
        //Apply any scale the exit portal may have (and apply custom exit scale)
        Vector3 exitScaleVector = targetPortal.GlobalTransform.Basis.Scale;
        Vector3 scaledAtExit = flipped / exitScaleVector * exitScale;
        //Convert from local space at the exit portal to global space
        Vector3 localAtExit = targetPortal.GlobalTransform.Basis * scaledAtExit;
        return localAtExit;
    }

    private int NonzeroSign(float value)
    {
        var s = Math.Sign(value);
        if (s == 0)
            s = 1;
        return s;
    }
}