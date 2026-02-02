using Godot;
using Netfox_Test.Scripts.Extensions;
using Netfox;
using Netfox.Logging;
using QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

namespace Netfox_Test.Scripts.Portals;

[GlobalClass]
public partial class PlayerPortalTraveller : PortalTraveller
{
    [Export] private CameraController localController;
    [Export] private CharacterBody3D body;

    private NetfoxLogger logger = NetfoxLogger.ForNetfox("NetworkRollback");

    public override void Travel(Portal entryPortal, Portal exitPortal)
    {
        var transformedVel = entryPortal.RealToExitDirection(body.Velocity);
        body.Position = entryPortal.RealToExitPosition(body.GlobalPosition);
        body.Velocity = transformedVel;
        body.FindChildByType<TickInterpolator>()?.Teleport();
        logger.LogWarning("Finished moving to: " + body.Position);

        var r1 = entryPortal.RealToExitTransform(localController.cameraMount.GlobalTransform).Basis.GetEuler();
        localController.horizontalView.RotateObjectLocal(Vector3.Down,
            localController.horizontalView.Transform.Basis.GetEuler().Y - r1.Y);
        localController.verticalView.RotateObjectLocal(Vector3.Left,
            localController.verticalView.Transform.Basis.GetEuler().X - r1.X);
    }

    public override bool HasCamera() => true;
}