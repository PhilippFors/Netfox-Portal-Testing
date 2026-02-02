using Godot;
using Netfox_Test.Scripts.Extensions;

namespace Netfox_Test.Scripts.Portals;

[GlobalClass]
public partial class PortalTraveller : Node
{
    [Export] protected Node3D positionNode;

    [Export] public bool isTravelling;

    public virtual void Travel(Portal entryPortal, Portal exitPortal)
    {
        var body = this.FindParentNode<CharacterBody3D>();
        if (body == null)
            return;

        var transformRelToThisPortal = entryPortal.GlobalTransform.AffineInverse() * body.GlobalTransform;
        var movedToOtherPortal = exitPortal.GlobalTransform * transformRelToThisPortal;
        body.GlobalTransform = movedToOtherPortal;
    }

    public virtual bool HasCamera() => false;

    public virtual Node3D GetPositionNode() => positionNode;
}