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
using static Cinemachine.CinemachineTriggerAction.ActionSettings;

namespace Network
{
    public class LobbyManager : SingletonNetwork<LobbyManager>
    {
        #region Constants
        const int MAX_PLAYERS = 4;
        const string CONNECTION_TYPE = "dtls";
        #endregion
        #region Variables
        private Lobby joinedLobby;
        private float temporizadorAtivacaoLobby;
        string playerName;
        bool isHost;
        #endregion
        #region Properties
        public Lobby JoinedLobby => joinedLobby;
        #endregion
        private async void Start()
        {

            // Dispara uma rotina de inicialização da API
            await UnityServices.InitializeAsync();

            // Escuta pelo evento de login do jogador
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Conectado como: " + AuthenticationService.Instance.PlayerId);
            };

            // Dispara uma rotina de login do usuario (de forma anônima)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            playerName = "Fulaninho" + Random.Range(1, 100);
            //UIHandler.instance.mainPanelPlayerNameInput.text = nomePlayer;
        }

        private void Update()
        {
            ManterLobbyAtivo();
        }

        #region Lobby Connection
        /* Mantem o Lobby atual sempre ativo --> evita o timeout de 3 segundos */
        /* A cada 15 segundos, manda um ping para o server da Unity  */
        private async void ManterLobbyAtivo()
        {
            if (IsHost && joinedLobby != null)
            {
                temporizadorAtivacaoLobby -= Time.deltaTime;

                if (temporizadorAtivacaoLobby < 0f)
                {
                    temporizadorAtivacaoLobby = 15f;
                    await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                }
            }
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
                    $"Número de players: {lobby.MaxPlayers}\tID: {lobby.Id}\tToken: {lobby.LobbyCode}\tPrivate? {lobby.IsPrivate}");
                NetworkManagerUI.Instance.EnableLobbyHUD();
                NetworkManagerUI.Instance.UpdateLobbyInfo(lobby);
                InvokeRepeating(nameof(HandleLobbyUpdates), 1.1f, 2.5f);
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
                    // Ordena de forma decrescente pela data de Criação
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
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                NetworkManagerUI.Instance.EnableLobbyHUD(false);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
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
            Debug.Log("Update");
            var lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            if (lobby.Players.Count != joinedLobby.Players.Count)
                NetworkManagerUI.Instance.UpdateLobbyInfo(lobby);
            joinedLobby = lobby;
            if (!isHost)
            {
                if (joinedLobby.Data["RelayCode"].Value != "0")
                    GameManager.Instance.JoinRelay(joinedLobby.Data["RelayCode"].Value);
            }
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
                // Tipo da Conexão: DTLS --> Conexão Segura
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
    }
    #endregion
}