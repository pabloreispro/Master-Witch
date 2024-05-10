using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Rendering;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.Rendering.Universal.Internal;
using WebSocketSharp;

public class NetworkManagerUI : SingletonNetwork<NetworkManagerUI>
{
    [Header("Start HUD")]
    [SerializeField] GameObject startBg;
    [Header("Network HUD")]
    [SerializeField] GameObject networkWindow;
    [SerializeField] TMP_InputField addressInputField;
    [SerializeField] TextMeshProUGUI lobbyCodeText;
    [SerializeField] Button serverButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;
    [SerializeField] Button startGameButton;
    [SerializeField] Button disconnectButton;
    [Header("Lobby HUD")]
    [SerializeField] LobbyItemUI lobbyItemPrefab;
    [SerializeField] TMP_InputField lobbyCode;
    [SerializeField] TMP_InputField lobbyName;
    [SerializeField] Toggle publicityToggle;
    [SerializeField] Transform lobbiesHolder;
    [Header("Final HUD")]
    [SerializeField] TextMeshProUGUI p1FinalScore;
    [SerializeField] TextMeshProUGUI p2FinalScore;
    [SerializeField] TextMeshProUGUI p3FinalScore;
    [SerializeField] TextMeshProUGUI p4FinalScore;
    private void Awake()
    {
        networkWindow.SetActive(true);
        serverButton.onClick.AddListener(StartServer);
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
        startGameButton.onClick.AddListener(StartGame);
        disconnectButton.onClick.AddListener(Disconnect);
        startGameButton.gameObject.SetActive(false);
        disconnectButton.gameObject.SetActive(false);
    }
    void UpdateHUD(bool isHost)
    {
        startGameButton.gameObject.SetActive(isHost);
        networkWindow.gameObject.SetActive(false);
        //disconnectButton.gameObject.SetActive(true);
    }
    public void UpdatePlayerScore(int playerID, float score)
    {
        switch (playerID)
        {
            case 0:
                p1FinalScore.text = score.ToString();
                break;
            case 1:
                p1FinalScore.text = score.ToString();
                break;
            case 2:
                p1FinalScore.text = score.ToString();
                break;
            case 3:
                p1FinalScore.text = score.ToString();
                break;
            default:
                break;
        }
    }
    #region Network
    public void StartGame()
    {
        GameManager.Instance.StartGame();
        startGameButton.gameObject.SetActive(false);
        startBg.SetActive(false);
        StartGameClientRpc();
    }
    [ClientRpc]
    void StartGameClientRpc()
    {
        startBg.SetActive(false);
    }
    public void ConnectToRelay()
    {
        LobbyManager.Instance.ClientRelay(lobbyCode.text.Substring(0, 6));
        UpdateHUD(false);
    }
    public void HostRelay()
    {
        LobbyManager.Instance.HostRelay();
        UpdateHUD(true);
    }
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        UpdateHUD(true);
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        UpdateHUD(true);
    }
    public void StartClient() 
    {
        TryConnectClient();
        UpdateHUD(false);
    }
    public void Disconnect()
    {
        NetworkManager.DisconnectClient(OwnerClientId);
    }
    private void TryConnectClient()
    {
        string ipAddress = addressInputField.text;
        if (ipAddress == null || ipAddress.Length == 0)
        {
            ipAddress = "127.0.0.1";
        }
        UnityTransport transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        transport.ConnectionData.Address = ipAddress;
        transport.ConnectionData.Port = ushort.Parse("7777");
        NetworkManager.Singleton.StartClient();
    }

    #region Lobby
    public void CreateLobby()
    {
        LobbyManager.Instance.CriaLobby(lobbyName.text, 4, publicityToggle.isOn, "Main");
        UpdateHUD(true);
    }
    public void UpdateLobbiesList() => ListLobbies();
    async void ListLobbies()
    {
        var lobbies = await LobbyManager.Instance.ListaLobbies();
        foreach (var lobby in lobbies)
        {
            var item = Instantiate(lobbyItemPrefab, lobbiesHolder);
            item.Initialize(lobby);
        }
    }
    public void JoinLobby()
    {
        if (lobbyCode.text.IsNullOrEmpty())
            Debug.Log($"Lobby code null");
        else
            Debug.Log($"Joining {lobbyCode.text.Substring(0, 6)}");
        LobbyManager.Instance.JoinLobbyByCode(lobbyCode.text.Substring(0, 6));
        UpdateHUD(false);
    }
    public void QuickJoinLobby()
    {
        LobbyManager.Instance.QuickJoinLobby();
        UpdateHUD(false);
    }
    public void UpdateLobbyCode(string lobbyCode)
    {
        lobbyCodeText.text = $"Lobby Code: {lobbyCode}";
    }
    #endregion
    #endregion
}