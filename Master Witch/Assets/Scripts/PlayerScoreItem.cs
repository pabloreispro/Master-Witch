using Network;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Leaderboard
{
    public class PlayerScoreItem : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI playerName;
        [SerializeField] GameObject[] divisions;
        [SerializeField] LeaderboardManager.ScoreCategory[] scoreCategories;

        internal void Initialize(int amount, PlayerNetworkData playerData, List<LeaderboardManager.LeaderboardCategory> categories)
        {
            EnableCategories(amount);
            playerName.text = playerData.PlayerName;
            for (int i = 0; i < scoreCategories.Length; i++)
            {
                scoreCategories[i].scoreCategoryText.text = categories[i].GetPlayerScore(playerData.PlayerIndex).ToString("00.00");
            }
        }
        void EnableCategories(int amount)
        {
            for (int i = 0; i < scoreCategories.Length; i++)
            {
                divisions[i].SetActive(i < amount);
                scoreCategories[i].SetActive(i < amount, false);
            }
        }

    }
}