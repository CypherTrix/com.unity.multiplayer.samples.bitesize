using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class NetworkManagerHud : NetworkBehaviour {
    [SerializeField] NetworkManager m_NetworkManager;

    UnityTransport m_Transport;

    // This is needed to make the port field more convenient. GUILayout.TextField is very limited and we want to be able to clear the field entirely so we can't cache this as ushort.
    string m_PortString = "7777";
    string m_ConnectAddress = "127.0.0.1";

    private static Dictionary<ulong, ClientPlayerData> clientPlayerData;

    private NetworkVariable<int> connectedPlayerCount = new();

    [SerializeField]
    UIDocument m_MainMenuUIDocument;

    [SerializeField]
    UIDocument m_InGameUIDocument;

    VisualElement m_MainMenuRootVisualElement;

    VisualElement m_InGameRootVisualElement;

    Button m_HostButton;

    Button m_ServerButton;

    Button m_ClientButton;

    Button m_ShutdownButton;

    TextField m_IPAddressField;

    TextField m_PortField;

    TextElement m_MenuStatusText;

    TextElement m_InGameStatusText;

    TextElement m_ConnectedPlayerCount;

    void Awake() {
        // Only cache networking manager but not transport here because transport could change anytime.
        // m_NetworkManager = NetworkManager.Singleton;

        m_MainMenuRootVisualElement = m_MainMenuUIDocument.rootVisualElement;

        m_IPAddressField = m_MainMenuRootVisualElement.Q<TextField>("IPAddressField");
        m_PortField = m_MainMenuRootVisualElement.Q<TextField>("PortField");
        m_HostButton = m_MainMenuRootVisualElement.Q<Button>("HostButton");
        m_ClientButton = m_MainMenuRootVisualElement.Q<Button>("ClientButton");
        m_ServerButton = m_MainMenuRootVisualElement.Q<Button>("ServerButton");
        m_MenuStatusText = m_MainMenuRootVisualElement.Q<TextElement>("ConnectionStatusText");

        m_InGameRootVisualElement = m_InGameUIDocument.rootVisualElement;
        m_ShutdownButton = m_InGameRootVisualElement.Q<Button>("ShutdownButton");
        m_InGameStatusText = m_InGameRootVisualElement.Q<TextElement>("InGameStatusText");
        m_ConnectedPlayerCount = m_InGameRootVisualElement.Q<TextElement>("ConnectedPlayerCount");

        m_IPAddressField.value = m_ConnectAddress;
        m_PortField.value = m_PortString;

        m_HostButton.clickable.clickedWithEventInfo += HostButtonClicked;
        m_ServerButton.clickable.clickedWithEventInfo += ServerButtonClicked;
        m_ClientButton.clickable.clickedWithEventInfo += ClientButtonClicked;
        m_ShutdownButton.clickable.clickedWithEventInfo += ShutdownButtonClicked;
    }

    private void OnClientCountChange(ulong clientID) {
        if (IsServer) {
            connectedPlayerCount.Value = m_NetworkManager.ConnectedClients.Count;
        }

    }

    private void UpdateConnectedPlayerCountDisplay(int oldvar, int newvar) {
        m_ConnectedPlayerCount.text = $"Player : {connectedPlayerCount.Value}";
    }

    void Start() {
        m_Transport = (UnityTransport)m_NetworkManager.NetworkConfig.NetworkTransport;

        ShowMainMenuUI(true);
        ShowInGameUI(false);
        ShowStatusText(false);

        m_NetworkManager.OnClientConnectedCallback += OnOnClientConnectedCallback;
        m_NetworkManager.OnClientDisconnectCallback += OnOnClientDisconnectCallback;

        m_NetworkManager.OnClientConnectedCallback += OnClientCountChange;
        m_NetworkManager.OnClientDisconnectCallback += OnClientCountChange;

        connectedPlayerCount.OnValueChanged += UpdateConnectedPlayerCountDisplay;
    }

    public static ClientPlayerData GetPlayerData(ulong clientId) {

        if (clientPlayerData.TryGetValue(clientId, out ClientPlayerData playerData)) {
            return playerData;
        } else {
            Debug.LogError($"Client ID '{clientId}' was not found in PlayerData.");
            return new ClientPlayerData($"Unknown : {clientId}", Color.white);
        }
    }

    void OnOnClientConnectedCallback(ulong clientId) {
       
        //if (clientPlayerData.TryAdd(m_NetworkManager.LocalClientId, new ClientPlayerData(GameManager.Instance.PlayerData.PlayerName, GameManager.Instance.PlayerData.PlayerColor))) {
        //    Debug.Log($"Add User Data {clientId} : {GameManager.Instance.PlayerData.PlayerName}");
        //} else {
        //    Debug.Log($"User ID {clientId} alreedy added");
        //}
        ShowMainMenuUI(false);
        ShowInGameUI(true);
    }

    void OnOnClientDisconnectCallback(ulong clientId) {
        if (m_NetworkManager.IsServer && clientId != NetworkManager.ServerClientId) {
            clientPlayerData.Remove(clientId);
            return;
        }
        ShowMainMenuUI(true);
        ShowInGameUI(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IsRunning(NetworkManager networkManager) => networkManager.IsServer || networkManager.IsClient;

    bool SetConnectionData() {
        m_ConnectAddress = SanitizeInput(m_IPAddressField.value);
        m_PortString = SanitizeInput(m_PortField.value);

        if (m_ConnectAddress == "") {
            m_MenuStatusText.text = "IP Address Invalid";
            StopAllCoroutines();
            StartCoroutine(ShowInvalidInputStatus());
            return false;
        }

        if (m_PortString == "") {
            m_MenuStatusText.text = "Port Invalid";
            StopAllCoroutines();
            StartCoroutine(ShowInvalidInputStatus());
            return false;
        }

        if (ushort.TryParse(m_PortString, out ushort port)) {
            m_Transport.SetConnectionData(m_ConnectAddress, port);
        } else {
            m_Transport.SetConnectionData(m_ConnectAddress, 7777);
        }
        return true;
    }

    static string SanitizeInput(string dirtyString) {
        // sanitize the input for the ip address
        return Regex.Replace(dirtyString, "[^0-9.]", "");
    }

    void HostButtonClicked(EventBase obj) {
        if (SetConnectionData()) {
            clientPlayerData = new();
            m_NetworkManager.StartHost();
        }
    }

    void ClientButtonClicked(EventBase obj) {
        if (SetConnectionData()) {
            m_NetworkManager.StartClient();
            StopAllCoroutines();
            StartCoroutine(ShowConnectingStatus());
        }
    }

    void ServerButtonClicked(EventBase obj) {
        if (SetConnectionData()) {
            clientPlayerData = new();
            m_NetworkManager.StartServer();
            ShowMainMenuUI(false);
            ShowInGameUI(true);
        }
    }

    void ShutdownButtonClicked(EventBase obj) {
        m_NetworkManager.Shutdown();
        ShowMainMenuUI(true);
        ShowInGameUI(false);
    }

    void ShowStatusText(bool visible) {
        m_MenuStatusText.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    IEnumerator ShowInvalidInputStatus() {
        ShowStatusText(true);

        yield return new WaitForSeconds(3f);

        ShowStatusText(false);
    }

    IEnumerator ShowConnectingStatus() {
        m_MenuStatusText.text = "Attempting to Connect...";
        ShowStatusText(true);

        m_HostButton.SetEnabled(false);
        m_ServerButton.SetEnabled(false);

        var unityTransport = m_NetworkManager.GetComponent<UnityTransport>();
        var connectTimeoutMs = unityTransport.ConnectTimeoutMS;
        var maxConnectAttempts = unityTransport.MaxConnectAttempts;

        yield return new WaitForSeconds(connectTimeoutMs * maxConnectAttempts / 1000f);

        // wait to verify connect status
        yield return new WaitForSeconds(1f);

        m_MenuStatusText.text = "Connection Attempt Failed";
        m_HostButton.SetEnabled(true);
        m_ServerButton.SetEnabled(true);

        yield return new WaitForSeconds(3f);

        ShowStatusText(false);
    }

    void ShowMainMenuUI(bool visible) {
        m_MainMenuRootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void ShowInGameUI(bool visible) {
        m_InGameRootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        if (m_NetworkManager.IsServer) {
            var mode = m_NetworkManager.IsHost ? "Host" : "Server";
            m_InGameStatusText.text = ($"ACTIVE ON PORT: {m_Transport.ConnectionData.Port}");
            m_ShutdownButton.text = ($"Shutdown {mode}");
        } else {
            if (m_NetworkManager.IsConnectedClient) {
                m_InGameStatusText.text = ($"CONNECTED {m_Transport.ConnectionData.Address} : {m_Transport.ConnectionData.Port.ToString()}");
                m_ShutdownButton.text = "Shutdown Client";
            }
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();
        if (m_HostButton != null) {
            m_HostButton.clickable.clickedWithEventInfo -= HostButtonClicked;
        }

        if (m_ServerButton != null) {
            m_ServerButton.clickable.clickedWithEventInfo -= ServerButtonClicked;
        }

        if (m_ClientButton != null) {
            m_ClientButton.clickable.clickedWithEventInfo -= ClientButtonClicked;
        }

        if (m_ShutdownButton != null) {
            m_ShutdownButton.clickable.clickedWithEventInfo -= ShutdownButtonClicked;
        }
        m_NetworkManager.OnClientConnectedCallback -= OnOnClientConnectedCallback;
        m_NetworkManager.OnClientDisconnectCallback -= OnOnClientDisconnectCallback;

        m_NetworkManager.OnClientConnectedCallback -= OnClientCountChange;
        m_NetworkManager.OnClientDisconnectCallback -= OnClientCountChange;

        connectedPlayerCount.OnValueChanged -= UpdateConnectedPlayerCountDisplay;
    }
}
