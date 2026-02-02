using Godot;

namespace Netfox_Test.Scripts.Networking;

public partial class MultiplayerRootClient : Node, IMultiplayerRootConnection
{
    private MultiplayerRoot multiplayerRoot;
    private IPeerWrapper wrapper;

    public void SetupMultiplayer(MultiplayerRoot root)
    {
        multiplayerRoot = root;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ServerDisconnected += OnDisconnectedFromServer;
        var args = multiplayerRoot.GetConnectionArgs();
        wrapper = new ENetWrapper();
        var err = wrapper.CreatePeer(args, Multiplayer);
        if (err != Error.Ok)
        {
            GD.PrintErr("MultiplayerRoot: Connection Failed.");
            return;
        }

        multiplayerRoot.AddToConnectedPlayers(Multiplayer.GetUniqueId());
    }

    public void StopMultiplayer()
    {
        multiplayerRoot.RpcId(1, nameof(multiplayerRoot.DisconnectPeer));
    }

    public void DisconnectPeer(int peer)
    {
    }

    private void OnDisconnectedFromServer()
    {
        Multiplayer.MultiplayerPeer.Close();
        Multiplayer.MultiplayerPeer = null;
        Multiplayer.ConnectedToServer -= OnConnectedToServer;
        Multiplayer.ServerDisconnected -= OnDisconnectedFromServer;
        multiplayerRoot.OnDisconnected();
    }

    private void OnConnectedToServer()
    {
        multiplayerRoot.RpcId(1, nameof(multiplayerRoot.GatherPlayerInformation));
        multiplayerRoot.OnConnected();
    }
}