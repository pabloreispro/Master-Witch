using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UI.Network {
    public class LobbyPlayerItem : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI playerName;
        public void Initialize(Unity.Services.Lobbies.Models.Player player)
        {
            playerName.text = player.Data["PlayerName"].Value;
        }
    }
}