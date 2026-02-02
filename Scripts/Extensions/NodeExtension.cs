using Godot;

namespace Netfox_Test.Scripts.Extensions;

public static class NodeExtension
{
	public static bool GetBool(this Node node, string key) => node._Get(key).AsBool();
	public static int GetInt(this Node node, string key) => node._Get(key).AsInt32();
	public static double GetDouble(this Node node, string key) => node._Get(key).AsDouble();

	public static T FindChildByType<T>(this Node node) where T : Node
	{
		var children = node.GetChildren();
		foreach (var child in children) {
			if (child is T res)
				return res;

			res = FindChildByType<T>(child);
			if (res != null)
				return res;
		}

		return null;
	}

	public static T FindParentNode<T>(this Node node) where T : Node
	{
		var parent = node.GetParent();
		while (parent != null) {
			if (parent is T res)
				return res;

			parent = parent.GetParent();
		}

		return null;
	}
}
