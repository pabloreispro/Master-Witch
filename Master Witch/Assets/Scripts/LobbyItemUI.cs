using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using Network;

namespace UI.Network
{
    public class LobbyItemUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI lobbyNameText;
        [SerializeField] TextMeshProUGUI playerAmountText;
        Lobby lobby;
        public void Initialize(Lobby newLobby)
        {
            lobby = newLobby;
            lobbyNameText.text = lobby.Name;
            playerAmountText.text = $"{4 - lobby.AvailableSlots} / 4";
        }
        public void ConnectToLobby()
        {
            LobbyManager.Instance.JoinLobbyByCode(lobby.Data["LobbyCode"].Value);
        }
    }
}