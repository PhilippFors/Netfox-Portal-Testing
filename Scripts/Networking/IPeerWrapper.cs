using Godot;

namespace Netfox_Test.Scripts.Networking;

public interface IPeerWrapper
{
    public Error CreatePeer(ConnectionArgs args, MultiplayerApi api);
}