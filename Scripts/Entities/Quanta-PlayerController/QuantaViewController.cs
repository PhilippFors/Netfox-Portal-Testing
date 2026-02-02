using Godot;
using QuantaDM.quanta_game.Scripts.Entities.Player_Controller;

namespace QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

public class QuantaViewController
{
    private PlayerParams playerParams;
    private QuantaPlayerController playerController;
    private CameraController localController;
    private CameraController rollbackController;
    //# Used for player view aesthetics such as view tilt and bobbing.

    public QuantaViewController(QuantaPlayerController playerController, PlayerParams playerParams,
        CameraController local)
    {
        this.playerController = playerController;
        this.playerParams = playerParams;
        localController = local;
    }

    public void CameraBob()
    {
        _CameraMountBob();

        var rot = localController.cameraMount.Rotation;
        localController.cameraMount.Rotation =
            new Vector3(rot.X, rot.Y, _CalcRoll(playerParams.ROLL_ANGLE, playerParams.ROLL_SPEED) * 2);
    }

    // Manipulates the Camera Mount gimbals.
    public void HandleLocalCameraInput(Vector2 lookInput)
    {
        localController.Rotate(lookInput);
    }

    public void HandleRollbackCameraInput(Vector2 lookInput)
    {
        rollbackController.Rotate(lookInput);
    }

    // Creates a sinusoidal Camera Mount bobbing motion whilst moving.
    private void _CameraMountBob()
    {
        float bob;
        Vector3 simvel;
        simvel = playerController.Velocity;
        simvel.Y = 0;

        if (playerParams.BOB_FREQUENCY == 0.0 || playerParams.BOB_FRACTION == 0)
        {
            return;
        }

        if (playerController.IsOnFloor())
        {
            bob = Mathf.Lerp(0,
                Mathf.Sin(Godot.Time.GetTicksMsec() * playerParams.BOB_FREQUENCY) / playerParams.BOB_FRACTION,
                simvel.Length() / 2) / playerParams.FORWARD_SPEED;
        }
        else
        {
            bob = 0;
        }

        var pos = localController.cameraMount.Position;
        localController.cameraMount.Position = new Vector3(pos.X, Mathf.Lerp(pos.Y, bob, 0.5f),
            pos.Z);
    }

    // Returns a value for how much the Camera Mount should tilt to the side.
    private float _CalcRoll(float rollangle, float rollspeed)
    {
        if (playerParams.ROLL_ANGLE == 0.0 || playerParams.ROLL_SPEED == 0)
        {
            return 0;
        }

        float side = playerController.Velocity.Dot(localController.horizontalView.Transform.Basis.X);

        float rollSign = (side < 0 ? 1 : -1);

        side = Mathf.Abs(side);

        float value = rollangle;

        if ((side < rollspeed))
        {
            side = side * value / rollspeed;
        }
        else
        {
            side = value;
        }

        return side * rollSign;
    }
}