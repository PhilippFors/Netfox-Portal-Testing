using System;
using Godot;
using Godot.Collections;
using QuantaDM.quanta_game.Scripts.Entities.Quanta_PlayerController;

namespace Netfox_Test.Scripts.Networking;

[GlobalClass]
public partial class MultiplayerRoot : Node
{
    public Action connected;
    public Action disconnected;
    
    [Export] private MultiplayerSpawner playerSpawner;
    [Export] private PackedScene player;
    
    public int UniqueId { get; private set; }
    private Dictionary<long, PlayerPeerInformation> connectedPlayersById = new();
    private ConnectionArgs connectionArgs;
    private IMultiplayerRootConnection connection;

    public void SetConnectionArgs(ConnectionArgs args) => connectionArgs = args;
    public ConnectionArgs GetConnectionArgs() => connectionArgs;

    public override void _Ready()
    {
        playerSpawner.SpawnFunction = new Callable(this, nameof(SpawnFunction));
    }

    public void StartConnection()
    {
        connection = connectionArgs.isServer ? new MultiplayerRootServer() : new MultiplayerRootClient();
        if(connectionArgs.isServer)
            Multiplayer.PeerConnected += OnPeerConnected;
        var node = connection as Node;
        node.Name = connection.GetType().ToString();
        AddChild(node, true);
        connection.SetupMultiplayer(this);
    }

    private void OnPeerConnected(long id)
    {
        SpawnPlayer(id);
    }

    public void OnConnected()
    {
        UniqueId = Multiplayer.GetUniqueId();
        connected?.Invoke();
        if(connectionArgs.isServer)
            SpawnPlayer(Multiplayer.GetUniqueId());
    }

    private void SpawnPlayer(long id)
    {
        var newPawn = playerSpawner.Spawn(id) as QuantaPlayerPawn;
        newPawn.rollbackSynchronizer.ProcessSettings();
    }

    public void StopConnection()
    {
        Multiplayer.PeerConnected -= OnPeerConnected;
        connection.StopMultiplayer();
    }
    
    protected Node SpawnFunction(Variant data)
    {
        int peerId = data.AsInt32();
        var instance = player.Instantiate<QuantaPlayerPawn>();
        instance.Name = "Player " + peerId;
        instance.uniqueId = peerId;
        instance.inputController.SetMultiplayerAuthority(peerId);
        instance.playerController.Velocity = Vector3.Zero;
        instance.playerController.SetPosition(new Vector3(0,1,0));
        return instance;
    }

    public void OnDisconnected()
    {
        disconnected?.Invoke();
    }

    public void AddToConnectedPlayers(long peerId)
    {
        PlayerPeerInformation info;
        if (!connectedPlayersById.ContainsKey(peerId))
        {
            info = new PlayerPeerInformation();
            connectedPlayersById.Add(peerId, info);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = true)]
    public void GatherPlayerInformation()
    {
        var peerId = Multiplayer.GetRemoteSenderId();
        AddToConnectedPlayers(peerId);
        var peerIds = new Array<long>();
        foreach (var players in connectedPlayersById)
        {
            peerIds.Add(players.Key);
        }

        GD.Print("Players Connected: " + connectedPlayersById.Count);
        Rpc(nameof(SyncPlayerInformation), peerIds);
    }

    public void RequestAuthority(Node node, long newAuthorityId, bool recursive = true)
    {
        GD.Print("Request Authority from: " + node.GetMultiplayerAuthority() + " to:" + newAuthorityId);
        RpcId(1, nameof(RequestAuthority_Server), node.GetPath(), newAuthorityId, recursive);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestAuthority_Server(string nodePath, long newAuthorityId, bool recursive)
    {
        var node = GetTree().Root.GetNode(nodePath);
        node.SetMultiplayerAuthority((int)newAuthorityId, recursive);
        Rpc(nameof(SyncAuthority), nodePath, newAuthorityId, recursive);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncAuthority(string nodePath, long newAuthorityId, bool recursive)
    {
        var node = GetTree().Root.GetNode(nodePath);
        GD.Print("Set Authority from: " + node.GetMultiplayerAuthority() + " to:" + newAuthorityId);
        node.SetMultiplayerAuthority((int)newAuthorityId, recursive);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncPlayerInformation(Array<long> peerId)
    {
        for (int i = 0; i < peerId.Count; i++)
        {
            AddToConnectedPlayers(peerId[i]);
        }
    }


    /// <summary>
    /// Disconnects the peer that sends the RPC
    /// </summary>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void DisconnectPeer()
    {
        var peer = Multiplayer.GetRemoteSenderId();
        connection.DisconnectPeer(peer);
    }
}