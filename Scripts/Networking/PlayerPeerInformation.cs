using Godot;

namespace Netfox_Test.Scripts.Networking;

[GlobalClass]
public partial class PlayerPeerInformation : GodotObject
{
	[Export] public string name;
	[Export] public ulong steamId;
	[Export] public GodotObject pawn;
	[Export] public bool ready;
}
