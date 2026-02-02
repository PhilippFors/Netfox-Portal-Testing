using Godot;
namespace QuantaDM.quanta_game.Scripts.Extensions;

public static class Node3DExtension
{
	public static void SetXPos(this Node3D node, float x)
	{
		node.SetPos(x, node.Position.Y, node.Position.Z);
	}
	public static void SetYPos(this Node3D node, float y)
	{
		node.SetPos(node.Position.X, y, node.Position.Z);
	}
	public static void SetZPos(this Node3D node, float z)
	{
		node.SetPos(node.Position.X, node.Position.Y, z);
	}
	public static void SetPos(this Node3D node, float x, float y, float z)
	{
		node.SetPosition(new Vector3(x, y, z));
	}

	public static void SetXRot(this Node3D node, float x)
	{
		var rot = node.Rotation;
		rot.X = x;
		node.Rotation = rot;
	}

	public static void SetYRot(this Node3D node, float y)
	{
		var rot = node.Rotation;
		rot.Y = y;
		node.Rotation = rot;
	}

	public static void SetZRot(this Node3D node, float z)
	{
		var rot = node.Rotation;
		rot.Z = z;
		node.Rotation = rot;
	}

	public static void SetOrigin(this Node3D node, Vector3 origin)
	{
		var transfem = node.Transform;
		transfem.Origin = origin;
		node.Transform = transfem;
	}

	public static void SetTransformY(this Node3D node, float y)
	{
		var trans = node.Transform;
		trans.Origin = new Vector3(trans.Origin.X, y, trans.Origin.Z);
		node.Transform = trans;
	}
}
