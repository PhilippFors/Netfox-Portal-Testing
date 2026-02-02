using Godot;
using Netfox;
using QuantaDM.quanta_game.Scripts.Entities.Player_Controller;

namespace QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

[GlobalClass]
public partial class QuantaPlayerPawn : Node
{
    public long uniqueId = -1;

    [Export] public RollbackSynchronizer rollbackSynchronizer;
    [Export] public QuantaPlayerController playerController;
    [Export] public QuantaInputController inputController;
    [Export] public Camera3D camera3D;
    [Export] public PlayerParams playerParams;
}