using System.Linq;
using Godot;

namespace Netfox_Test.Scripts.Networking;

public partial class MultiplayerRootServer : Node, IMultiplayerRootConnection
{
    private MultiplayerRoot multiplayerRoot;

    public void SetupMultiplayer(MultiplayerRoot root)
    {
        multiplayerRoot = root;
        var args = multiplayerRoot.GetConnectionArgs();
        var err = new ENetWrapper().CreatePeer(args, Multiplayer);
        if (err != Error.Ok)
        {
            GD.PrintErr("MultiplayerRoot: Connection Failed. Error:" + err);
            return;
        }

        multiplayerRoot.AddToConnectedPlayers(Multiplayer.GetUniqueId());
        multiplayerRoot.OnConnected();
    }

    public void StopMultiplayer()
    {
        var peers = Multiplayer.GetPeers();
        foreach (int peer in peers)
            DisconnectPeer(peer);

        Multiplayer.MultiplayerPeer.Close();
        Multiplayer.MultiplayerPeer = null;
        multiplayerRoot.OnDisconnected();
    }

    public void DisconnectPeer(int peer)
    {
        if (!multiplayerRoot.GetConnectionArgs().isServer)
            return;

        var peers = Multiplayer.GetPeers().ToList();
        if (peers.Contains(peer))
            Multiplayer.MultiplayerPeer.DisconnectPeer(peer);
    }
}