using Godot;
using Netfox_Test.Scripts.Networking.Lobbies.ENet;

namespace Netfox_Test.Scripts.Networking;

public class ENetWrapper : IPeerWrapper
{
    public Error CreatePeer(ConnectionArgs args, MultiplayerApi api)
    {
        return args.isServer ? CreateServer(args, api) : CreateClient(args, api);
    }

    private Error CreateServer(ConnectionArgs args, MultiplayerApi api)
    {
        var peer = new ENetMultiplayerPeer();
        if (args.lobbyData is ENetLobbyData lobby)
        {
            Error err = peer.CreateServer(lobby.port);
            api.SetMultiplayerPeer(peer);
            return err;
        }

        GD.PrintErr("Expected ENetLobbyData. Did not get :(");
        return Error.Failed;
    }

    private Error CreateClient(ConnectionArgs args, MultiplayerApi api)
    {
        var peer = new ENetMultiplayerPeer();
        if (args.lobbyData is ENetLobbyData lobby)
        {
            Error err = peer.CreateClient(lobby.address, lobby.port);
            api.SetMultiplayerPeer(peer);
            return err;
        }

        GD.PrintErr("Expected ENetLobbyData. Did not get :(");
        return Error.Failed;
    }
}