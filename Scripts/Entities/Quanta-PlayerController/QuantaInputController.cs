using Godot;
using Netfox;
using QuantaDM.quanta_game.Scripts.Entities.Player_Controller;

namespace QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

[GlobalClass]
public partial class QuantaInputController : Node
{
    [Export] public QuantaPlayerController PlayerController;
    [Export] public PlayerParams PlayerParams;
    
    [Export] public Vector2 movementInput;
    [Export] public Vector2 mouseInput;
    [Export] public Vector3 moveDir;
    [Export] public bool jumpOn;
    [Export] public bool duckOn;
    [Export] public bool teleport;
    public Vector2 mouseInputBuffer;
    private bool jumpBuffer;
    private bool duckBuffer;
    private bool teleportBuffer;

    public override void _Ready()
    {
        // Input.UseAccumulatedInput = true;
        NetfoxSharp.NetworkTime.BeforeTickLoop += GatherInputBeforeTick;
        NetfoxSharp.NetworkTime.AfterTick += GatherInputAfterTick;
    }

    public override void _ExitTree()
    {
        NetfoxSharp.NetworkTime.BeforeTickLoop -= GatherInputBeforeTick;
        NetfoxSharp.NetworkTime.AfterTick -= GatherInputAfterTick;
    }

    public override void _Process(double delta)
    {
        if (!IsMultiplayerAuthority())
            return;
        jumpBuffer = Input.IsActionPressed("pm_jump") ? PlayerParams.AUTOHOP : Input.IsActionPressed("pm_jump");
        if (Input.IsActionJustPressed("teleport"))
        {
            teleportBuffer = true;
        }
    }

    private void GatherInputBeforeTick()
    {
        if (!IsMultiplayerAuthority())
            return;

        // Get input strength on the horizontal axes.
        var ix = Input.GetActionRawStrength("pm_moveright") - Input.GetActionRawStrength("pm_moveleft");
        var iy = Input.GetActionRawStrength("pm_movebackward") - Input.GetActionRawStrength("pm_moveforward");

        // Collect input.
        movementInput = new Vector2(ix, iy).Normalized();

        // Gather the horizontal speeds.
        var speeds = new Vector2(PlayerParams.SIDE_SPEED, PlayerParams.FORWARD_SPEED);

        // Clamp down the horizontal speeds to MAX_SPEED.
        foreach (int i in GD.Range(2))
        {
            if (speeds[i] > PlayerParams.MAX_SPEED)
            {
                speeds[i] *= PlayerParams.MAX_SPEED / speeds[i];
            }
        }

        // Create vector that stores speed and direction.
        moveDir = new Vector3(movementInput.X * speeds.X, 0, movementInput.Y * speeds.Y).Rotated(Vector3.Up,
            PlayerController.localController.horizontalView.Rotation.Y);

        // Bring down the move direction to a third of it's speed.
        if (PlayerController.ducked)
        {
            moveDir *= PlayerParams.DUCKING_SPEED_MULTIPLIER;
        }

        // Clamp desired speed to max speed
        if (moveDir.Length() > PlayerParams.MAX_SPEED)
        {
            moveDir *= PlayerParams.MAX_SPEED / moveDir.Length();
        }

        mouseInput = mouseInputBuffer * (float)NetworkTime.PhysicsFactor;
        mouseInputBuffer = Vector2.Zero;
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (!IsMultiplayerAuthority())
            return;
        //---------------------
        // Replace with your own implementation of MOUSE_MODE switching!!
        //---------------------
        if (Input.MouseMode != Input.MouseModeEnum.Captured)
        {
            if (inputEvent is InputEventKey)
            {
                if (inputEvent.IsActionPressed("ui_cancel"))
                {
                    PlayerController.GetTree().Quit();
                }
            }

            if (inputEvent is InputEventMouseButton button)
            {
                if (button.ButtonIndex == MouseButton.Left)
                {
                    Input.SetMouseMode(Input.MouseModeEnum.Captured);
                }
            }

            return;
        }

        if (inputEvent is InputEventKey)
        {
            if (inputEvent.IsActionPressed("ui_cancel"))
            {
                Input.SetMouseMode(Input.MouseModeEnum.Visible);
            }

            return;
        }

        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                // Grab the event data and process it.
                GetMouseInput(mouseMotion);
            }
        }
    }

    private void GetMouseInput(InputEventMouseMotion inputEvent)
    {
        // Deform the mouse input to make it viewport size independent.
        var viewportTransform = PlayerController.GetTree().Root.GetFinalTransform();
        mouseInputBuffer += ((InputEventMouseMotion)inputEvent.XformedBy(viewportTransform)).Relative;

        var degreesPerUnit = 0.0001f;

        // Modify mouse input based on sensitivity and granularity.
        mouseInputBuffer *= PlayerParams.MOUSE_SENSITIVITY;
        mouseInputBuffer *= degreesPerUnit;
    }

    private void GatherInputAfterTick(double delta, long tick)
    {
        if (!IsMultiplayerAuthority())
            return;
        jumpOn = jumpBuffer;
        jumpBuffer = false;
        duckOn = duckBuffer;
        duckBuffer = false;
        teleport = teleportBuffer;
        teleportBuffer = false;
    }
}