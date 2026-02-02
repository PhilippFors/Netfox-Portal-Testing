using Godot;

//@icon("src/gdticon.png")


namespace QuantaDM.quanta_game.Scripts.Entities.Player_Controller;

[GlobalClass]
public partial class PlayerParams : Resource
{
	public enum BunnyhopCapMode { NONE, THRESHOLD, DROP, }

	[ExportGroup("Configuration")]
	[Export] public bool AUTOHOP = false; //# Tells the character to check for the jump action being held instead of pressed, which will make all jumps perfect bunny hops.

	[ExportGroup("Engine Dependant Variables")]
	[Export] public float FORWARD_SPEED = 10.16f; //# Forward and backward move speed. The default value equals 8.128 (or 320 Hammer units/inches).
	[Export] public float SIDE_SPEED = 10.16f; //# Left and right move speed. The default value equals 8.128 (or 320 Hammer units/inches).
	[Export] public float MAX_SPEED = 8.128f; //# Maximum speed your inputs have in their directions. The default value equals 8.128 (or 320 Hammer units/inches).
	[Export] public float MAX_AIR_SPEED = 0.762f; //# The maximum speed you can accelerate to in the [method _airaccelerate] function. The default value equals 0.762 (or 30 Hammer units/inches).
	[Export] public float STOP_SPEED = 2.54f; //# Speed threshold for stopping in the [method _friction] function. The default value equals 2.540 (or 100 Hammer units/inches).
	[Export] public float GRAVITY = 20.32f; //# Speed of gravity. The default value equals 20.320 (or 800 Hammer units/inches).
	[Export] public float JUMP_HEIGHT = 1.143f; //# Height of the player's jump. The default value equals 1.143 (or 45 Hammer units/inches).
	[Export] public float STEP_HEIGHT = 0.457f; //# Maximum height of a stair step. The default value equals 0.457 (or 18 Hammer units/inches).

	[ExportSubgroup("Player Dimensions")]
	[Export] public Vector3 HULL_STANDING_BOUNDS = new Vector3(0.813f, 1.829f, 0.813f); //# The dimensions of the player's collision hull while standing. The default dimensions are [0.813, 1.829, 0.813] (or [32, 72, 32] in Hammer units/inches).
	[Export] public Vector3 HULL_DUCKING_BOUNDS = new Vector3(0.813f, 0.914f, 0.813f); //# The dimensions of the player's collision hull while ducking. The default dimensions are [0.813, 0.914, 0.813] (or [32, 36, 32] in Hammer units/inches).
	[Export] public float VIEW_OFFSET = 0.711f; //# How much the camera hovers from player origin while standing. The default value equals 0.711 (or 28 Hammer units/inches).
	[Export] public float DUCK_VIEW_OFFSET = 0.305f; //# How much the camera hovers from player origin while crouching. The default value equals 0.305 (or 12 Hammer units/inches).

	[ExportGroup("Engine Agnostic Variables")]
	[Export] public float ACCELERATION = 10.0f; //# The base acceleration amount that is multiplied by [code]wishspeed[/code] inside of [method _accelerate]. The default value equals 10.
	[Export] public float AIR_ACCELERATION = 10.0f; //# The base acceleration amount that is multiplied by [code]wishspeed[/code] inside of [method _airaccelerate]. The default value equals 10.
	[Export] public float FRICTION = 4.0f; //# The multiplier of dropped speed when friction is acting on the player. The default value equals 4.
	[Export] public float DUCKING_SPEED_MULTIPLIER = 0.333f; //# The multiplier placed on the player's desired input speed while ducking. The default value equals 0.333.
	[ExportSubgroup("Bunny-hop Cap")]
	[Export] public BunnyhopCapMode BUNNYHOP_CAP_MODE = BunnyhopCapMode.NONE; //# How the player responds when jumping while past the speed threshold.
	[Export] public float SPEED_THRESHOLD_FACTOR = 1.7f; //# How many times over [code]MAX_SPEED[/code] the player can have when performing a jump.
	[Export] public float SPEED_DROP_FACTOR = 1.1f; //# How many times over [code]MAX_SPEED[/code] the player can have when performing a jump.

	[ExportGroup("Camera")]
	[Export] public float MOUSE_SENSITIVITY = 12; //# How fast the camera moves in response to player input. The default value equals 15.
	[Export] public float BOB_FREQUENCY = 0.008f;
	[Export] public float BOB_FRACTION = 12f;
	[Export] public float ROLL_ANGLE = 0.65f;
	[Export] public float ROLL_SPEED = 300f;
}
