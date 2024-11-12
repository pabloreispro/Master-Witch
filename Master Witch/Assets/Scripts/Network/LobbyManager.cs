using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;
using WebSocketSharp;
using UnityEngine.UI;
using UI;
using System.Linq;

namespace Network
{
    public class LobbyManager : SingletonNetworkPersistent<LobbyManager>
    {
        #region Constants
        const int MAX_PLAYERS = 4;
        const string CONNECTION_TYPE = "dtls";
        #endregion
        #region Variables
        private Lobby joinedLobby;
        private float temporizadorAtivacaoLobby;
        public string playerName;
        #endregion
        #region Properties
        public Lobby JoinedLobby => joinedLobby;
        #endregion
        private async void Start()
        {
            // Dispara uma rotina de inicializa��o da API
            await UnityServices.InitializeAsync();

            // Escuta pelo evento de login do jogador
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Conectado como: " + AuthenticationService.Instance.PlayerId);
            };

            // Dispara uma rotina de login do usuario (de forma an�nima)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
            NetworkManager.ConnectionApprovalCallback = ConnectionApprovalCallback;
            //UIHandler.instance.mainPanelPlayerNameInput.text = nomePlayer;
            //GameManager.Instance.InitializeGame();
        }
        void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {

            response.Approved = true;
            response.CreatePlayerObject = false;
            //int i = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count % SceneManager.Instance.spawnPlayersMarket.Count;
            //response.Position = SceneManager.Instance.spawnPlayersMarket.ElementAt(i).position;
            response.Pending = false;
            //response.Rotation = Quaternion.Euler(0f,180f,0f);
        }

        #region Lobby Connection
        /* Mantem o Lobby atual sempre ativo --> evita o timeout de 3 segundos */
        /* A cada 15 segundos, manda um ping para o server da Unity  */
        private async void SendHeartbeat()
        {
            if (NetworkManager.IsHost && joinedLobby != null)
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
        }
        public async void CreateLobby(string lobbyName, bool isPrivate, string gameMode)
        {
            try
            {
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = new Unity.Services.Lobbies.Models.Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)},
                        }
                    },
                    Data = new Dictionary<string, DataObject>
                    {
                        {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode,
                       DataObject.IndexOptions.S1)},
                    }
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MAX_PLAYERS, createLobbyOptions);
                lobby = await LobbyService.Instance.UpdateLobbyAsync(
                lobby.Id,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {"LobbyCode", new DataObject(DataObject.VisibilityOptions.Public, lobby.LobbyCode) },
                        {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode,DataObject.IndexOptions.S1)},
                        {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0", DataObject.IndexOptions.S2)},
                    }
                });
                joinedLobby = lobby;
                Debug.Log($"O lobby {lobby.Name} foi criado por {playerName}\t" +
                    $"N�mero de players: {lobby.MaxPlayers}\tID: {lobby.Id}\tToken: {lobby.LobbyCode}\tPrivate? {lobby.IsPrivate}");
                NetworkManagerUI.Instance.EnableLobbyHUD();
                NetworkManagerUI.Instance.UpdateLobbyInfo(lobby);
                InvokeRepeating(nameof(HandleLobbyUpdates), 1.1f, 2.5f);
                InvokeRepeating(nameof(SendHeartbeat), 15f, 15f);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }
        public async Task<List<Lobby>> ListaLobbies(QueryLobbiesOptions queryLobbiesOptions = null)
        {
            try
            {
                QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
                Debug.Log($"Encontrei {queryResponse.Results.Count} lobbie(s):");
                int i = 1;
                foreach (Lobby lobby in queryResponse.Results)
                {
                    Debug.Log($"\tLobby[{i}]: {lobby.Name}\tLobby Code: {lobby.Data["LobbyCode"].Value}");
                    MostraInformacoesPlayers(lobby);
                    i++;
                }
                return queryResponse.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
                return null;
            }

        }
        public void FiltraListaLobbies(string availableSlots)
        {
            try
            {
                QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
                {
                    // Retorna os primeiros 25 lobbies encontrados com o filtro
                    Count = 25,
                    Filters = new List<QueryFilter>
                {
                    // Filtra lobbies com AvailableSlots > availableSlots (greater than)
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, availableSlots, QueryFilter.OpOptions.GT),
                    //new QueryFilter(QueryFilter.FieldOptions.S1, "<Modo de Jogo>", QueryFilter.OpOptions.EQ)
                },
                    Order = new List<QueryOrder>
                {
                    // Ordena de forma decrescente pela data de Cria��o
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
                };
                //ListaLobbies(queryLobbiesOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        public async void JoinLobbyByCode(string lobbyCode)
        {
            try
            {
                JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
                {
                    Player = new Unity.Services.Lobbies.Models.Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
                    }
                    }
                };
                Debug.Log($"Entrando no lobby {lobbyCode}");
                Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
                joinedLobby = lobby;
                NetworkManagerUI.Instance.EnableLobbyHUD();
                NetworkManagerUI.Instance.UpdateLobbyInfo(lobby);
                MostraInformacoesPlayers(lobby);
                InvokeRepeating(nameof(HandleLobbyUpdates), 1.1f, 2.5f);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        public async void QuickJoinLobby()
        {
            try
            {
                JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
                {
                    Player = new Unity.Services.Lobbies.Models.Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
                        }
                    }
                };

                Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
                joinedLobby = lobby;
                NetworkManagerUI.Instance.EnableLobbyHUD();
                NetworkManagerUI.Instance.UpdateLobbyInfo(lobby);
                MostraInformacoesPlayers(lobby);
                InvokeRepeating(nameof(HandleLobbyUpdates), 1.1f, 2.5f);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }
        public async void LeaveLobby()
        {
            if (joinedLobby == null) return;
            if (NetworkManager.IsHost)
            {
                CloseLobby();
            }
            else
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"Erro ao sair do lobby: {e.Message}");
                }
            }
            joinedLobby = null;
            CancelInvoke();
        }
        public void ResetJoinedLobby() => joinedLobby = null;
        public async void CloseLobby()
        {
            if (joinedLobby == null) return;
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                joinedLobby = null;
                CancelInvoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Erro ao deletar o lobby: {e.Message}");
            }
        }
        private void MostraInformacoesPlayers(Lobby lobby)
        {
            Debug.Log($"Mostrando informacoes dos jogadores do lobby {lobby.Name}");
            foreach (Unity.Services.Lobbies.Models.Player player in lobby.Players)
            {
                Debug.Log($"\tNome: {player.Data["PlayerName"].Value}\tID: {player.Id}");
                // Debug.Log($"\tID: {player.Id}");
            }
        }
        async void HandleLobbyUpdates()
        {
            if (joinedLobby == null) return;
            var lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            if (lobby.Players.Count != joinedLobby.Players.Count)
                NetworkManagerUI.Instance.UpdateLobbyInfo(lobby);
            joinedLobby = lobby;
            if (!NetworkManager.IsHost)
            {
                if (joinedLobby.Data["RelayCode"].Value != "0")
                    NetworkManagerUI.Instance.JoinRelay(joinedLobby.Data["RelayCode"].Value);
            }
            Debug.Log($"Update {joinedLobby.Data["RelayCode"].Value}     Lobby: {joinedLobby?.Id}");
        }
        public void UpdatePlayerName(string name)
        {
            playerName = name;
        }
        #endregion
        #region Relay Connection

        public async Task<string> StartHostRelay()
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, CONNECTION_TYPE));
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(
                joinedLobby.Id,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode, DataObject.IndexOptions.S2)},
                    }
                });
                CancelInvoke();
                return NetworkManager.Singleton.StartHost() ? joinCode : null;
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                throw;
            }
        }
        public async void ClientRelay(string joinCode)
        {
            await StartClientWithRelay(joinCode);
        }
        public async Task<bool> StartClientWithRelay(string joinCode)
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
                // Tipo da Conex�o: DTLS --> Conex�o Segura
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, CONNECTION_TYPE));
                CancelInvoke();
                return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                throw;
            }
        }
        //Server-side
        public void OnClientsReady()
        {
            PlayerNetworkManager.Instance.SetPlayerInfo();
            CloseLobby();
            SceneLoader.Instance.ServerLoadLevel(SceneLoader.Scenes.Game);
        }

#pragma warning disable UNT0006 // Incorrect message signature
        private void OnPlayerDisconnected(ulong clientId)
#pragma warning restore UNT0006 // Incorrect message signature
        {
            Debug.Log($"Player {clientId} desconectado.");
            //// Verifica se o host foi desconectado
            //if (clientId == NetworkManager.ServerClientId)
            DisconnectFromServer();
        }
        public void DisconnectFromServer()
        {
            NetworkManager.Singleton.Shutdown();
            PlayerNetworkManager.Instance.ResetPlayerList();
            SceneLoader.Instance.LoadLevel(SceneLoader.Scenes.Menu);
        }
    }
    #endregion
}