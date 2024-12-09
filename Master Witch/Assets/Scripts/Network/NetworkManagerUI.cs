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
using System.Linq;


namespace UI
{
    public class NetworkManagerUI : SingletonNetwork<NetworkManagerUI>
    {
        const string DEFAULT_PLAYER_NAME = "Mage ";
        const string PLAYER_NAME_KEY = "PlayerName";
        [Header("HUDs")]
        [SerializeField] GameObject networkHUD;
        [SerializeField] GameObject networkOptionsHUD;
        [SerializeField] GameObject mainLobbyHUD;
        [SerializeField] GameObject createLobbyHUD;
        [SerializeField] GameObject joinLobbyHUD;
        [SerializeField] GameObject lobbyHUD;
        [SerializeField] GameObject windowsHolder;
        [Header("Tutorial HUD")]
        int currentTutorial = 0;
        [SerializeField] GameObject[] tutorialImages;
        [Header("Network HUD")]
        [SerializeField] TMP_InputField addressInputField;
        [SerializeField] TextMeshProUGUI lobbyCodeText;
        [SerializeField] TMP_InputField playerNameIF;
        [SerializeField] GameObject noLobbiesText;
        [SerializeField] Button updateLobbyListBT;
        [SerializeField] Button hostButton;
        [SerializeField] Button clientButton;
        [SerializeField] Button startGameButton;
        [Header("Lobby List HUD")]
        [SerializeField] LobbyItemUI lobbyItemPrefab;
        [SerializeField] TMP_InputField lobbyCode;
        [SerializeField] TMP_InputField newLobbyName;
        [SerializeField] Toggle publicityToggle;
        [SerializeField] Transform lobbiesHolder;
        [Header("Lobby HUD")]
        [SerializeField] TextMeshProUGUI lobbyName;
        [SerializeField] LobbyPlayerItem lobbyPlayerItemPrefab;
        [SerializeField] Transform playerList;
        List<GameObject> lobbyList = new List<GameObject>();


        private void Awake()
        {
            networkHUD.SetActive(true);
            startGameButton.onClick.AddListener(StartGame);
        }
        private void Start()
        {
            if(PlayerPrefs.GetString(PLAYER_NAME_KEY).Equals(""))
                playerNameIF.text = DEFAULT_PLAYER_NAME + Random.Range(0, 999);
            else
                playerNameIF.text = PlayerPrefs.GetString(PLAYER_NAME_KEY);
            UpdatePlayerName(playerNameIF.text);
        }

        public void EnableLobbyHUD(bool enabled = true)
        {
            lobbyHUD.SetActive(enabled);
            networkOptionsHUD.SetActive(!enabled);
            mainLobbyHUD.SetActive(!enabled);
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
        public void EnableMenu()
        {
            networkHUD.SetActive(true);
            networkOptionsHUD.SetActive(true);
            windowsHolder.SetActive(false);
        }
        public void NextTutorial()
        {
            if (currentTutorial >= tutorialImages.Length - 1) return;
            tutorialImages[currentTutorial].SetActive(false);
            currentTutorial++;
            tutorialImages[currentTutorial].SetActive(true);
        }
        public void PreviousTutorial()
        {
            if (currentTutorial <= 0) return;
            tutorialImages[currentTutorial].SetActive(false);
            currentTutorial--;
            tutorialImages[currentTutorial].SetActive(true);
        }

        public void QuitGame(){
            SceneLoader.Instance.CloseGame();
        }
        
        #region Network
        public void StartGame()
        {
            HostRelay();
        }
        public async void HostRelay()
        {
            EnableHUD(false);
            await LobbyManager.Instance.StartHostRelay();
            if (NetworkManager.IsHost)
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
        private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            PlayerNetworkManager.Instance.OnSceneLoaded(sceneName, loadSceneMode, clientsCompleted, clientsTimedOut);
        }

        public void JoinRelay(string joinCode)
        {
            if (NetworkManager.IsHost) return;
            EnableHUD(false);
            StartClientRelay(joinCode);
        }
        async void StartClientRelay(string joinCode)
        {
            if (NetworkManager.IsHost) return;
            EnableHUD(false);
            Debug.Log($"start relay {joinCode}");
            await LobbyManager.Instance.StartClientWithRelay(joinCode);
            Debug.Log($"join");
        }
        public void UpdatePlayerName(string name)
        {
            LobbyManager.Instance.UpdatePlayerName(name);
            PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
        }
        #region Lobby
        public void UpdateLobbiesList()
        {
            updateLobbyListBT.interactable = false;
            ListLobbies();
            Invoke(nameof(EnableUpdateButton), 3f);
        }
        void EnableUpdateButton() => updateLobbyListBT.interactable = true;
        async void ListLobbies()
        {
            noLobbiesText.SetActive(false);
            var lobbies = new List<Lobby>();
            lobbies = await LobbyManager.Instance.ListaLobbies();
            if (lobbyList.Count > 0)
            {
                for (int i = 0; i < lobbyList.Count; i++)
                {
                    Destroy(lobbyList[i]);
                }
            }
            foreach (var lobby in lobbies)
            {
                var item = Instantiate(lobbyItemPrefab, lobbiesHolder);
                item.Initialize(lobby);
                lobbyList.Add(item.gameObject);
            }
            noLobbiesText.SetActive(lobbies.Count == 0);
        }
        public void CreateLobby()
        {
            LobbyManager.Instance.CreateLobby(newLobbyName.text, publicityToggle.isOn, "Main");
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
            lobbyName.text = lobby.Name;
            lobbyCodeText.text = $"{lobby.LobbyCode}";
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
                pHud.Initialize(player, players.IndexOf(player) == 0);
            }
        }
        public void LeaveLobby()
        {
            LobbyManager.Instance.LeaveLobby();
            //EnableLobbyHUD(false);
        }

        #endregion
        #endregion

        #region Tutorial
        public void StartTutorial()
        {
            LobbyManager.Instance.CreateLobby("TutorialLobby", false, "Tutorial");
            StartGame();
        }
        #endregion
    }
}