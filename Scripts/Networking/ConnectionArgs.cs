using Netfox_Test.Scripts.Networking.Lobbies;

namespace Netfox_Test.Scripts.Networking;

public struct ConnectionArgs
{
    public ConnectionType connectionType;
    public bool isServer;
    public ILobbyData lobbyData;
}