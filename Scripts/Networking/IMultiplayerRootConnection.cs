namespace Netfox_Test.Scripts.Networking;

public interface IMultiplayerRootConnection
{
    public void SetupMultiplayer(MultiplayerRoot root);
    public void StopMultiplayer();
    public void DisconnectPeer(int peer);
}