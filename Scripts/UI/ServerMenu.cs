using Godot;
using Netfox_Test.Scripts.Networking;
using Netfox_Test.Scripts.Networking.Lobbies.ENet;
using MultiplayerRoot = Netfox_Test.Scripts.Networking.MultiplayerRoot;

namespace Netfox_Test.Scripts.UI;

[GlobalClass]
public partial class ServerMenu : Control
{
    [Export] private MultiplayerRoot root;

    public override void _Ready()
    {
        root.connected += HideMenu;
        root.disconnected += ShowMenu;
    }

    public void JoinGame()
    {
        root.SetConnectionArgs(new ConnectionArgs()
        {
            connectionType = ConnectionType.ENet,
            isServer = false,
            lobbyData = new ENetLobbyData()
            {
                address = "127.0.0.1",
                port = 6969
            }
        });
        root.StartConnection();
    }

    public void HostGame()
    {
        root.SetConnectionArgs(new ConnectionArgs()
        {
            connectionType = ConnectionType.ENet,
            isServer = true,
            lobbyData = new ENetLobbyData()
            {
                address = "127.0.0.1",
                port = 6969
            }
        });
        root.StartConnection();
    }

    private void ShowMenu()
    {
        Show();
    }

    private void HideMenu()
    {
        Hide();
    }
}