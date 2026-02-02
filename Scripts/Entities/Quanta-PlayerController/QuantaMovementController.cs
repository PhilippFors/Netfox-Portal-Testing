using Godot;
using QuantaDM.quanta_game.Scripts.Entities.Player_Controller;

namespace QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

public class QuantaMovementController
{
    private QuantaPlayerController playerController;
    private PlayerParams playerParams;

    public QuantaMovementController(QuantaPlayerController controller, PlayerParams playerParams)
    {
        playerController = controller;
        this.playerParams = playerParams;
    }

    public void Accelerate(double delta, Vector3 wishdir, float wishspeed, float accel)
    {
        if (playerController == null)
            return;

        double addspeed;
        double accelspeed;
        double currentspeed;

        // See if we are changing direction a bit
        currentspeed = playerController.Velocity.Dot(wishdir);

        // Reduce wishspeed by the amount of veer.
        addspeed = wishspeed - currentspeed;

        // If not going to add any speed, done.
        if (addspeed <= 0)
            return;

        // Determine the amount of acceleration.
        accelspeed = accel * wishspeed * delta;

        // Cap at addspeed
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        // Adjust velocity.
        playerController.Velocity += (float)accelspeed * wishdir;
    }

    // Adds to the player's velocity based on direction, speed and acceleration. 
    // The difference between _accelerate() and this function is it caps the maximum speed you can accelerate to.
    public void AirAccelerate(double delta, Vector3 wishdir, double wishspeed, double accel)
    {
        if (playerController == null)
            return;

        double addspeed;
        double accelspeed;
        double currentspeed;
        var wishspd = wishspeed;

        if (wishspd > playerParams.MAX_AIR_SPEED)
            wishspd = playerParams.MAX_AIR_SPEED;


        // See if we are changing direction a bit
        currentspeed = playerController.Velocity.Dot(wishdir);

        // Reduce wishspeed by the amount of veer.
        addspeed = wishspd - currentspeed;

        // If not going to add any speed, done.
        if (addspeed <= 0)
            return;

        // Determine the amount of acceleration.
        accelspeed = accel * wishspeed * delta;

        // Cap at addspeed
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        // Adjust velocity.
        playerController.Velocity += (float)accelspeed * wishdir;
    }

    // Applies friction to the player's horizontal velocity
    public void Friction(double delta, double strength)
    {
        if (playerController == null)
            return;

        var speed = playerController.Velocity.Length();

        // Bleed off some speed, but if we have less that the bleed
        // threshold, bleed the threshold amount.
        var control = speed < playerParams.STOP_SPEED ? playerParams.STOP_SPEED : speed;

        // Add the amount to the drop amount
        var drop = control * (playerParams.FRICTION * strength) * delta;

        // Scale the velocity.
        var newspeed = speed - drop;

        if (newspeed < 0)
            newspeed = 0;

        if (speed > 0)
            newspeed /= speed;

        var vel = playerController.Velocity;
        playerController.Velocity = new Vector3(vel.X * (float)newspeed, vel.Y, vel.Z * (float)newspeed);
    }

    // Applies a jump force to the player.
    public void Jump(double delta)
    {
        // Apply the jump impulse
        var vel = playerController.Velocity;
        var jumpVel = Mathf.Sqrt(2 * playerParams.GRAVITY * playerParams.JUMP_HEIGHT);
        playerController.Velocity = new Vector3(vel.X, jumpVel, vel.Z);

        // Add in some gravity correction
        playerController.Velocity -= new Vector3(0, playerParams.GRAVITY, 0) * (float)delta * 0.5f;

        // If the Player Parameters wants us to clip the velocity, do it.

        switch (playerParams.BUNNYHOP_CAP_MODE)
        {
            case PlayerParams.BunnyhopCapMode.NONE:
                break;
            case PlayerParams.BunnyhopCapMode.THRESHOLD:
                BunnyhopCapmodeThreshold();
                break;
            case PlayerParams.BunnyhopCapMode.DROP:
                BunnyhopCapmodeDrop();
                break;
        }
    }

    // Crops horizontal velocity down to a defined maximum threshold.
    public void BunnyhopCapmodeThreshold()
    {
        double spd;
        double fraction;
        double maxscaledspeed;

        // Calculate what the maximum speed is.
        maxscaledspeed = playerParams.SPEED_THRESHOLD_FACTOR * playerParams.MAX_SPEED;

        // Avoid divide-by-zero errors.
        if (maxscaledspeed <= 0)
            return;

        spd = new Vector3(playerController.Velocity.X, 0, playerController.Velocity.Z).Length();

        if (spd <= maxscaledspeed)
            return;

        fraction = maxscaledspeed / spd;

        var vel = playerController.Velocity;
        playerController.Velocity = new Vector3(vel.X * (float)fraction, vel.Y, vel.Z * (float)fraction);
    }

    // Crops horizontal velocity down to a defined dropped amount.
    public void BunnyhopCapmodeDrop()
    {
        double spd;
        double fraction;
        double maxscaledspeed;
        double dropspeed;

        maxscaledspeed = playerParams.SPEED_THRESHOLD_FACTOR * playerParams.MAX_SPEED;
        dropspeed = playerParams.SPEED_DROP_FACTOR * playerParams.MAX_SPEED;

        if (maxscaledspeed <= 0)
            return;

        spd = new Vector3(playerController.Velocity.X, 0, playerController.Velocity.Z).Length();

        if (spd <= maxscaledspeed)
            return;

        fraction = dropspeed / spd;

        var vel = playerController.Velocity;
        playerController.Velocity = new Vector3(vel.X * (float)fraction, vel.Y, vel.Z * (float)fraction);
    }
}