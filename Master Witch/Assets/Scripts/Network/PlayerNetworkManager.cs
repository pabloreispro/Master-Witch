using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class PlayerNetworkManager : SingletonNetwork<PlayerNetworkManager>
    {
        Dictionary<ulong, Player> playerList = new Dictionary<ulong, Player>();
        Dictionary<Player, ulong> idList = new Dictionary<Player, ulong>();
        [Header("Player Materials")]
        [SerializeField] Material player1Mat;
        [SerializeField] Material player2Mat;
        [SerializeField] Material player3Mat;
        [SerializeField] Material player4Mat;

        public Dictionary<ulong, Player> GetPlayer => playerList;
        public Dictionary<Player, ulong> GetID => idList;
        Dictionary<ulong, bool> playersReady = new Dictionary<ulong, bool>();
        private static HashSet<ulong> readyClients = new HashSet<ulong>();
        void Awake()
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
            for(int i =0; i<playerList.Count; i++){
                if(GetPlayerByIndex(i)==playerIndex){
                    return i;
                }
            }
            return -1;
        }

        void OnClientConnected(ulong playerID)
        {
            if (!IsServer) return;
            Debug.Log("aqui");
            var player = NetworkManager.SpawnManager.GetPlayerNetworkObject(playerID).GetComponent<Player>();
            
            Debug.Log($"Connected id: {playerID}, NO ID: {player.NetworkObjectId}, NB ID {player.NetworkBehaviourId}");
            playerList.Add(playerID, player);
            idList.Add(player, playerID);
            OnPlayerConnect();
            OnClientConnectedClientRpc(playerID, playerList.Keys.ToArray());
            if(playerList.Count>=LobbyManager.Instance.JoinedLobby.Players.Count){
                SignalClientReady(NetworkManager.Singleton.LocalClientId);
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
                switch (i)
                {
                    case 0:
                        playerList.ElementAt(i).Value.OnConnected(player1Mat, i);
                        break;
                    case 1:
                        playerList.ElementAt(i).Value.OnConnected(player2Mat, i);
                        break;
                    case 2:
                        playerList.ElementAt(i).Value.OnConnected(player3Mat, i);
                        break;
                    case 3:
                        playerList.ElementAt(i).Value.OnConnected(player4Mat, i);
                        break;
                    default:
                        break;
                }
            }
           
    }


    void SignalClientReady(ulong clientId)
    {
        readyClients.Add(clientId);
        if (readyClients.Count >= LobbyManager.Instance.JoinedLobby.Players.Count)
        {
            GameManager.Instance.OnClientsReady();
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
            if(!IsServer)
                playerList.Remove(playerID);
        }
    }
}