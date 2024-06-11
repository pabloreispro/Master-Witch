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
using UI.Network;

using Network;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using System.Runtime.InteropServices;

namespace UI
{
    public class NetworkManagerUI : SingletonNetwork<NetworkManagerUI>
    {
        [Header("HUDs")]
        [SerializeField] GameObject gameHUD;
        [SerializeField] GameObject networkHUD;
        [SerializeField] GameObject networkOptionsHUD;
        [SerializeField] GameObject createLobbyHUD;
        [SerializeField] GameObject joinLobbyHUD;
        [SerializeField] GameObject lobbyHUD;
        [Header("Network HUD")]
        [SerializeField] TMP_InputField addressInputField;
        [SerializeField] TextMeshProUGUI lobbyCodeText;
        [SerializeField] Button hostButton;
        [SerializeField] Button clientButton;
        [SerializeField] Button startGameButton;
        [SerializeField] Button disconnectButton;
        [Header("   Lobby List HUD")]
        [SerializeField] LobbyItemUI lobbyItemPrefab;
        [SerializeField] TMP_InputField lobbyCode;
        [SerializeField] TMP_InputField lobbyName;
        [SerializeField] Toggle publicityToggle;
        [SerializeField] Transform lobbiesHolder;
        [Header("   Lobby HUD")]
        [SerializeField] LobbyPlayerItem lobbyPlayerItemPrefab;
        [SerializeField] Transform playerList;
        [Header("Final HUD")]
        [SerializeField] TextMeshProUGUI[] playerFinalScore;
        [SerializeField] TextMeshProUGUI[] textScore;
        public Toggle[] playerFinalCheck;
        public GameObject finalPanel;
        public GameObject finalResult;
        [SerializeField] TextMeshProUGUI[] namePlayerResult;
        private void Awake()
        {
            networkHUD.SetActive(true);
            //hostButton.onClick.AddListener(StartHost);
            //clientButton.onClick.AddListener(StartClient);
            //disconnectButton.onClick.AddListener(Disconnect);
            startGameButton.onClick.AddListener(StartGame);
            disconnectButton.gameObject.SetActive(false);
        }

        public void EnableLobbyHUD(bool enabled = true)
        {
            lobbyHUD.SetActive(enabled);
            networkOptionsHUD.SetActive(!enabled);
            joinLobbyHUD.SetActive(false);
            createLobbyHUD.SetActive(false);
            if (enabled)
                startGameButton.gameObject.SetActive(AuthenticationService.Instance.PlayerId == LobbyManager.Instance.JoinedLobby.HostId);
        }
        public void EnableHUD(bool enabled)
        {
            lobbyHUD.SetActive(enabled);
            networkOptionsHUD.SetActive(enabled);
        }
        [ClientRpc]
        public void OnGameStartedClientRpc()
        {
            gameHUD.SetActive(true);
            networkHUD.SetActive(false);
            EliminationPlayer.Instance.AddScoresPlayers();
            GameManager.Instance.numberPlayer = PlayerNetworkManager.Instance.GetPlayer.Count;
        }
        [ClientRpc]
        public void OnGameFinalClientRpc(){
            finalPanel.gameObject.SetActive(true);
        }
        public void UpdatePlayerScore(int playerID, float score)
        {
            switch (playerID)
            {
                case 0:
                    textScore[0].text = score.ToString();
                    break;
                case 1:
                    textScore[1].text = score.ToString();
                    break;
                case 2:
                    textScore[2].text = score.ToString();
                    break;
                case 3:
                    textScore[3].text = score.ToString();
                    break;
                default:
                    break;
            }
        }
        #region Network
        public void StartGame()
        {
            GameManager.Instance.HostRelay();
        }
        //public void StartHost()
        //{
        //    NetworkManager.Singleton.StartHost();
        //    OnGameStarted();
        //}
        //public void StartClient()
        //{
        //    TryConnectClient();
        //    OnGameStarted();
        //}
        //private void TryConnectClient()
        //{
        //    string ipAddress = addressInputField.text;
        //    if (ipAddress == null || ipAddress.Length == 0)
        //    {
        //        ipAddress = "127.0.0.1";
        //    }
        //    UnityTransport transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        //    transport.ConnectionData.Address = ipAddress;
        //    transport.ConnectionData.Port = ushort.Parse("7777");
        //    NetworkManager.Singleton.StartClient();
        //}
        //public void Disconnect()
        //{
        //    NetworkManager.DisconnectClient(OwnerClientId);
        //}
        #region Lobby
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
        public void CreateLobby()
        {
            LobbyManager.Instance.CreateLobby(lobbyName.text, publicityToggle.isOn, "Main");
        }
        public void JoinLobby()
        {
            if (lobbyCode.text.IsNullOrEmpty())
            {
                Debug.Log($"Lobby code null");
                return;
            }
            else
                Debug.Log($"Joining {lobbyCode.text.Substring(0, 6)}");
            LobbyManager.Instance.JoinLobbyByCode(lobbyCode.text.Substring(0, 6));
        }
        public void QuickJoinLobby()
        {
            LobbyManager.Instance.QuickJoinLobby();
        }

        public void UpdateLobbyInfo(Lobby lobby)
        {
            lobbyCodeText.text = $"Lobby Code: {lobby.LobbyCode}";
            ListPlayers(lobby.Players);
        }
        void ListPlayers(List<Unity.Services.Lobbies.Models.Player> players)
        {
            for (int i = 0; i < playerList.childCount; i++)
            {
                Destroy(playerList.GetChild(i).gameObject);
            }
            foreach (var player in players)
            {
                var pHud = Instantiate(lobbyPlayerItemPrefab, playerList);
                pHud.Initialize(player);
            }
        }

        #endregion
        #endregion

        [ServerRpc(RequireOwnership =false)]
        public void UpdateFinalScreenServerRpc(){
            UpdateFinalScreenClientRpc();
        }
        [ClientRpc]
        public void UpdateFinalScreenClientRpc(){
            for(int i = 0; i<GameManager.Instance.numberPlayer; i++){
                playerFinalScore[i].gameObject.SetActive(true);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateToggleServerRpc(int playerID, bool toggleValue){
            UpdateToggleClientRpc(playerID, toggleValue);
            EndRound.Instance.CanNextRound();
        }

        [ClientRpc]
        public void UpdateToggleClientRpc(int playerID, bool toggleValue){
            playerFinalCheck[playerID].isOn = toggleValue;
        }

        [ClientRpc]
        public void ActiveFinalPanelClientRpc(){
            finalPanel.SetActive(true);
            UpdateFinalScreenServerRpc();
            
        }

        public void UpdateFinalResult(int playerID)
        {
            string name = PlayerNetworkManager.Instance.GetPlayerByIndex(playerID).name;
            switch (playerID)
            {
                case 0:
                    namePlayerResult[0].text = name;
                    break;
                case 1:
                    namePlayerResult[1].text = name;
                    break;
                case 2:
                    namePlayerResult[2].text = name;
                    break;
                default:
                    break;
            }
        }
        
    }
}