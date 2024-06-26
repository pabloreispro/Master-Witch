using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UI.Network {
    public class LobbyPlayerItem : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI playerName;
        [SerializeField] GameObject hostIcon;
        public void Initialize(Unity.Services.Lobbies.Models.Player player, bool isHost)
        {
            playerName.text = player.Data["PlayerName"].Value;
            hostIcon.SetActive(isHost);
        }
    }
}