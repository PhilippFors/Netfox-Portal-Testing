using Godot;
namespace QuantaDM.quanta_game.scripts.Extensions;

public static class CsgBoxExtension
{
	public static void SetSize(this CsgBox3D box, float x, float y, float z)
	{
		box.SetSize(new Vector3(x, y, z));
	}

	public static void SetSizeX(this CsgBox3D box, float x)
	{
		box.SetSize(x, box.Size.Y, box.Size.Z);
	}

	public static void SetSizeY(this CsgBox3D box, float y)
	{
		box.SetSize(box.Size.X, y, box.Size.Z);
	}

	public static void SetSizeZ(this CsgBox3D box, float z)
	{
		box.SetSize(box.Size.X, box.Size.Y, z);
	}
}
