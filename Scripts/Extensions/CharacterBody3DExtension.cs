using Godot;
namespace QuantaDM.quanta_game.Scripts.Extensions;

public static class CharacterBody3DExtension
{
	public static void SetVelY(this CharacterBody3D body, float y)
	{
		var vel = body.Velocity;
		vel.Y = y;
		body.Velocity = vel;
	}

	public static void SetVelX(this CharacterBody3D body, float x)
	{
		var vel = body.Velocity;
		vel.X = x;
		body.Velocity = vel;
	}

	public static void SetVelZ(this CharacterBody3D body, float z)
	{
		var vel = body.Velocity;
		vel.Z = z;
		body.Velocity = vel;
	}

	public static void SetVel(this CharacterBody3D body, float x, float y, float z)
	{
		var vec = new Vector3(x, y, z);
		body.Velocity = vec;
	}

	public static void SetVel(this CharacterBody3D body, Vector3 vec)
	{
		body.Velocity = vec;
	}
}
