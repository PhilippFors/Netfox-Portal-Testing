using Godot;

namespace QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

[GlobalClass]
public partial class CameraController : Node3D
{
    [Export] public Node3D horizontalView;
    [Export] public Node3D verticalView;
    [Export] public Node3D cameraMount;

    public void Rotate(Vector2 lookInput)
    {
        horizontalView.RotateObjectLocal(Vector3.Down, lookInput.X);
        horizontalView.Orthonormalize();

        verticalView.RotateObjectLocal(Vector3.Left, lookInput.Y);
        verticalView.Orthonormalize();

        var rot = verticalView.Rotation;
        verticalView.Rotation =
            new Vector3(Mathf.Clamp(verticalView.Rotation.X, -1.57f, 1.57f), rot.Y, rot.Z);
        verticalView.Orthonormalize();
    }
}