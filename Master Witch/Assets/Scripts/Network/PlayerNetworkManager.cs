using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using Unity.Netcode;
using UnityEngine;
using Game.SceneGame;

namespace Network
{
    public class PlayerNetworkManager : SingletonNetworkPersistent<PlayerNetworkManager>
    {
        Dictionary<ulong, Player> playerList = new Dictionary<ulong, Player>();
        Dictionary<Player, ulong> idList = new Dictionary<Player, ulong>();
        [SerializeField] Player playerPrefab;
        [Header("Player Materials")]
        [SerializeField] Material player1Mat;
        [SerializeField] Material player2Mat;
        [SerializeField] Material player3Mat;
        [SerializeField] Material player4Mat;
        List<PlayerNetworkData> playersData = new List<PlayerNetworkData>();
        public Dictionary<ulong, Player> GetPlayer => playerList;
        public Dictionary<Player, ulong> GetID => idList;
        public List<PlayerNetworkData> PlayersData => playersData;
        public int PlayersCount => playersData.Count;
        public Material Player1Mat => player1Mat;
        public Material Player2Mat => player2Mat;
        public Material Player3Mat => player3Mat;
        public Material Player4Mat => player4Mat;
        public ulong LocalNetworkID => NetworkManager.Singleton.LocalClientId;
        //Dictionary<ulong, bool> playersReady = new Dictionary<ulong, bool>();
        //private static HashSet<ulong> readyClients = new HashSet<ulong>();
        void Start()
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public Player GetPlayerByIndex(int playerIndex)
        {
            if (playerIndex >= playerList.Count) return null;
            return playerList.ElementAt(playerIndex).Value;
        }

        public int GetPlayerIndexID(Player playerIndex)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (GetPlayerByIndex(i) == playerIndex)
                {
                    return i;
                }
            }
            return -1;
        }


        // USAR LISTA DE PLAYER DATA PARA INCLUIR DADOS QUANDO INICIAR PARTIDA NO LOBBY

        //public void SetPlayerInfo()
        //{
        //    var players = LobbyManager.Instance.JoinedLobby.Players;
        //    playersData = new PlayerNetworkData[players.Count];
        //    for (int i = 0; i < players.Count; i++)
        //    {
        //        var playerName = players[i].Data["PlayerName"].Value;
        //        var playerData = new PlayerNetworkData(i, playerName, PlayerNetworkData.PlayerNetworkStatus.Ready);
        //        playersData[i] = playerData;
        //        Debug.Log(playerData);
        //    }
        //    SetPlayersDataClientRpc(playersData.ToArray());
        //}
        [ClientRpc]
        void SetPlayersDataClientRpc(PlayerNetworkData[] data)
        {
            playersData = new List<PlayerNetworkData>(data);
        }
        //When connected to Relay, match starting
        void OnClientConnected(ulong playerNetworkId)
        {
            if (!IsServer) return;
            //var player = NetworkManager.SpawnManager.GetPlayerNetworkObject(playerID).GetComponent<Player>();

            //Debug.Log($"Connected id: {playerID}, NO ID: {player.NetworkObjectId}, NB ID {player.NetworkBehaviourId}");
            //playerList.Add(playerID, player);
            //idList.Add(player, playerID);
            //OnPlayerConnect();
            //OnClientConnectedClientRpc(playerID, playerList.Keys.ToArray());
            //if (playerList.Count >= LobbyManager.Instance.JoinedLobby.Players.Count)
            //{
            var player = LobbyManager.Instance.JoinedLobby.Players.Last();
            var id = playersData.Count;
            var playerName = player.Data["PlayerName"].Value;
            var playerData = new PlayerNetworkData(id, playerName, playerNetworkId, PlayerNetworkData.PlayerNetworkStatus.Ready);
            playersData.Add(playerData);
            Debug.Log(playerData);
            SetPlayersDataClientRpc(playersData.ToArray());
            if (NetworkManager.Singleton.ConnectedClientsList.Count >= LobbyManager.Instance.JoinedLobby.Players.Count)
            {
                //readyClients.Add(playerID);
                LobbyManager.Instance.OnClientsReady();
            }
        }
        public void OnSceneLoaded(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (sceneName.Equals(SceneLoader.Scenes.Game.ToString()))
            {
                SpawnPlayers();
            }
        }
        public void SpawnPlayers()
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
            {
                //int i = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count % SceneManager.Instance.spawnPlayersMarket.Count;
                //response.Position =;
                var playerID = NetworkManager.Singleton.ConnectedClientsIds[i];
                Player player = Instantiate(playerPrefab, SceneManager.Instance.spawnPlayersMarket.ElementAt(i).position, playerPrefab.transform.rotation);
                NetworkObject networkObject = player.GetComponent<NetworkObject>();
                networkObject.SpawnAsPlayerObject(playerID, true);
                playerList.Add(playerID, player);
                idList.Add(player, playerID);
                OnPlayerConnect();
                OnClientConnectedClientRpc(playerID, playerList.Keys.ToArray());
            }
        }
        [ClientRpc]
        void OnClientConnectedClientRpc(ulong playerID, ulong[] keys)
        {
            if (IsServer) return;
            playerList = new Dictionary<ulong, Player>();
            var players = FindObjectsOfType<Player>();
            for (int i = 0; i < keys.Length; i++)
            {
                for (int j = 0; j < players.Length; j++)
                {
                    if (players[j].OwnerClientId == keys[i])
                    {
                        Debug.Log($"{keys[i]}, {players[j].NetworkObjectId}");
                        playerList.Add(keys[i], players[j]);
                        break;
                    }
                }
            }
            idList = new Dictionary<Player, ulong>();
            for (int i = 0; i < playerList.Count; i++)
            {
                idList.Add(playerList.Values.ToArray()[i], playerList.Keys.ToArray()[i]);
            }
            OnPlayerConnect();
        }
        void OnPlayerConnect()
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                Material material = player1Mat;
                switch (i)
                {
                    case 0:
                        material = player1Mat;
                        break;
                    case 1:
                        material = player2Mat;
                        break;
                    case 2:
                        material = player3Mat;
                        break;
                    case 3:
                        material = player4Mat;
                        break;
                    default:
                        break;
                }
                playerList.ElementAt(i).Value.OnConnected(material, i);
                //GameManager.Instance.ChangeBenchColor(material, i);
            }
        }


        void OnClientDisconnected(ulong playerID)
        {
            playerList.Remove(playerID);
            OnClientDisconnectedClientRpc(playerID);
        }
        [ClientRpc]
        void OnClientDisconnectedClientRpc(ulong playerID)
        {
            if (!IsServer)
                playerList.Remove(playerID);
        }
        public void ResetPlayerList()
        {
            playerList.Clear();
        }
    }
    public struct PlayerNetworkData : INetworkSerializable
    {
        int playerIndex;
        string playerName;
        ulong playerNetworkID;
        //int acessory01Id;
        //int acessory02Id;
        PlayerNetworkStatus status;
        public int PlayerIndex => playerIndex;
        public string PlayerName => playerName;
        public ulong PlayerNetworkID => playerNetworkID;
        public PlayerNetworkStatus Status => status;
        public PlayerNetworkData(int playerIndex, string playerName, ulong playerNetworkID, PlayerNetworkStatus status)
        {
            this.playerIndex = playerIndex;
            this.playerNetworkID = 0;
            this.playerName = playerName;
            this.status = status;
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerIndex);
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref playerNetworkID);
            //serializer.SerializeValue(ref acessory01Id);
            //serializer.SerializeValue(ref acessory02Id);
            serializer.SerializeValue(ref status);
        }

        public void SetNewStatus(PlayerNetworkStatus newStatus)
        {
            switch (newStatus)
            {
                case PlayerNetworkStatus.Unknown:
                    break;
                case PlayerNetworkStatus.Ready:
                    break;
                case PlayerNetworkStatus.NotReady:
                    break;
                case PlayerNetworkStatus.Loading:
                    break;
                case PlayerNetworkStatus.Waiting:
                    break;
                default:
                    break;
            }
            status = newStatus;
        }
        public override string ToString()
        {
            return $"Player Network Data:" +
                $"\n Name: {playerName}" +
                $"\n Index: {playerIndex}" +
                $"\n Network ID: {playerNetworkID}" +
                $"\n Status {status}";
        }

        public enum PlayerNetworkStatus
        { 
            Unknown,
            Ready,
            NotReady,
            Loading,
            Waiting,
        }
    }
}