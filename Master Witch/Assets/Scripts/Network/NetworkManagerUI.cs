using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Rendering;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.Rendering.Universal.Internal;

public class NetworkManagerUI : SingletonNetwork<NetworkManagerUI>
{
    [Header("Start HUD")]
    [SerializeField] GameObject startBg;
    [Header("Network HUD")]
    [SerializeField] GameObject networkWindow;
    [SerializeField] TMP_InputField addressInputField;
    [SerializeField] Button serverButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;
    [SerializeField] Button startGameButton;
    [SerializeField] Button disconnectButton;
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
    void UpdateHUD()
    {
        startGameButton.gameObject.SetActive(IsHost);
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
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        UpdateHUD();
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        UpdateHUD();
    }
    public void StartClient() 
    {
        TryConnectClient();
        UpdateHUD();
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
    #endregion


}
