using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class PlayerNetworkManager : SingletonNetwork<PlayerNetworkManager>
    {
        public Dictionary<ulong, Player> playerList = new Dictionary<ulong, Player>();
        [Header("Player Materials")]
        [SerializeField] Material player1Mat;
        [SerializeField] Material player2Mat;
        [SerializeField] Material player3Mat;
        [SerializeField] Material player4Mat;
        // Start is called before the first frame update
        void Awake()
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public Player GetPlayerByID(ulong playerID) => playerList[playerID];
        public Player GetPlayerByIndex(int playerIndex)
        {
            if (playerIndex >= playerList.Count) return null;
            return playerList.ElementAt(playerIndex).Value;
        }


        void OnClientConnected(ulong playerID)
        {
            if (!IsServer) return;
            Debug.Log("aqui");
            var player = NetworkManager.SpawnManager.GetPlayerNetworkObject(playerID).GetComponent<Player>();
            Debug.Log($"Connected id: {playerID}, NO ID: {player.NetworkObjectId}, NB ID {player.NetworkBehaviourId}");
            playerList.Add(playerID, player);
            OnPlayerConnect();
            OnClientConnectedClientRpc(playerID, playerList.Keys.ToArray());
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
            OnPlayerConnect();
        }
        void OnPlayerConnect()
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        playerList.ElementAt(i).Value.OnConnected(player1Mat);
                        break;
                    case 1:
                        playerList.ElementAt(i).Value.OnConnected(player2Mat);
                        break;
                    case 2:
                        playerList.ElementAt(i).Value.OnConnected(player3Mat);
                        break;
                    case 3:
                        playerList.ElementAt(i).Value.OnConnected(player4Mat);
                        break;
                    default:
                        break;
                }
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