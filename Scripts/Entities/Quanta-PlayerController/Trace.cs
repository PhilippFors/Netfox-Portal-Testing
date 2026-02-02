using Godot;
namespace QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

public partial class Trace : RefCounted
{
	public Vector3 endPos;
	public float fraction;
	public Vector3 normal;
	public Godot.Collections.Array SurfaceFlags;

	public Trace()
	{
		endPos = Vector3.Zero;
		fraction = 1;
		normal = Vector3.Up;
		SurfaceFlags = new Godot.Collections.Array();
	}

	public Trace(Vector3 endPos, float fraction, Vector3 normal)
	{
		this.endPos = endPos;
		this.fraction = fraction;
		this.normal = normal;
	}

	public Trace SetEndPos(Vector3 end_pos)
	{
		endPos = end_pos;
		return this;
	}

	public Trace SetNormal(Vector3 normal)
	{
		this.normal = normal;
		return this;
	}

	public Trace SetFraction(float fraction)
	{
		this.fraction = fraction;
		return this;
	}
}
