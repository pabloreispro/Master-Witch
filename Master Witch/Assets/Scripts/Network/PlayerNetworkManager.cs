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
            playerList.Add(playerID, NetworkManager.SpawnManager.GetPlayerNetworkObject(playerID).GetComponent<Player>());
            Debug.Log("aqui");
            OnClientConnectedClientRpc(playerID);
        }
        [ClientRpc]
        void OnClientConnectedClientRpc(ulong playerID)
        {
            if(!IsServer)
                playerList.Add(playerID, NetworkManager.SpawnManager.GetPlayerNetworkObject(playerID).GetComponent<Player>());
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