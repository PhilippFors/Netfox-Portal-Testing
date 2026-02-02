using Godot;
using QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

namespace QuantaDM.quanta_game.Scripts.Entities.Player_Controller;

[GlobalClass]
public partial class QuantaPlayerCamera : Node3D
{

    [Export] public Camera3D Camera;
    [Export] private QuantaInputController InputController;

    public override void _Ready()
    {
        if (!InputController.IsMultiplayerAuthority())
            return;

        Camera.Current = true;
    }
}