using Unity.Netcode;
public class NetworkUI : NetworkBehaviour {

    private NetworkVariable<int> connectedPlayerCount = new(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);


    private void Update() {
        if (!IsServer) return;
        connectedPlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }

}

