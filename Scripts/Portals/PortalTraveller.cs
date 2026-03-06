using Godot;
using Netfox_Test.Scripts.Extensions;
using Netfox;

namespace Netfox_Test.Scripts.Portals;

[GlobalClass]
public partial class PortalTraveller : Node
{
    [Export] protected Node3D positionNode;

    [Export] public bool isTravelling;
    [Export] public long teleportTick;
    [Export] public long processedTeleportTick;
    [Export] public Vector3 exitPosition;
    [Export] public Vector3 exitVelocity;
    [Export] public Vector3 exitEuler;

    public void _rollback_tick(double delta, long tick, bool isFresh)
    {
        if(isTravelling && teleportTick >= tick)
        {
            Travel();
            isTravelling = false;
        }
    }

    public virtual void Travel()
    {
        // var body = this.FindParentNode<CharacterBody3D>();
        // if (body == null)
        //     return;
        //
        // var transformRelToThisPortal = entryPortal.GlobalTransform.AffineInverse() * body.GlobalTransform;
        // var movedToOtherPortal = exitPortal.GlobalTransform * transformRelToThisPortal;
        // body.GlobalTransform = movedToOtherPortal;
    }

    public void StartTeleport(Vector3 position, Vector3 velocity, Vector3 euler)
    {
        exitPosition = position;
        exitVelocity = velocity;
        exitEuler = euler;
        isTravelling = true;
        teleportTick = NetworkRollback.Tick;
        // Travel();
    }
    
    public virtual bool HasCamera() => false;

    public virtual Node3D GetPositionNode() => positionNode;
}