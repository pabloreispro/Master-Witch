using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.UI;
using Game.SO;
using Network;
using TMPro;
using System;
using Unity.Netcode;
using UnityEngine.Experimental.AI;
using static UnityEngine.Rendering.DebugUI;

namespace UI.Leaderboard
{
    public class LeaderboardManager : Singleton<LeaderboardManager>
    {
        const int ADDITIONAL_CATEGORIES = 2;
        public Dictionary<int, float> FinalScores = new Dictionary<int, float>();

        [SerializeField] TextMeshProUGUI roundLabel;
        [Header("Header Components")]
        [SerializeField] GameObject[] divisions;
        [SerializeField] ScoreCategory[] scoreCategories;
        [Header("Player Components")]
        [SerializeField] PlayerScoreItem[] playerScoreItems;
        [Header("Confirmation Components")]
        [SerializeField] Image[] notReadyIcons;
        [SerializeField] Image[] readyIcons;
        [Header("Assets")]
        [SerializeField] protected Sprite clockSprite;
        [SerializeField] protected Sprite equalsSprite;
        List<LeaderboardCategory> leaderboardCategories = new List<LeaderboardCategory>();
        bool playerIsReady;
        public void EnableRoundLeaderboard(bool finalRound)
        {
            leaderboardCategories = new List<LeaderboardCategory>();
            int categoriesAmount;
            if (!finalRound)
            {
                roundLabel.text = "Round " + NumberToRoman(GameManager.Instance.CurrentRound);
                categoriesAmount = GameManager.Instance.Chefs.Count + ADDITIONAL_CATEGORIES;
                EnableCategories(categoriesAmount);
                for (int i = 0; i < categoriesAmount; i++)
                {
                    if (i < GameManager.Instance.Chefs.Count)
                        leaderboardCategories.Add(new ChefLeaderboardCategory(scoreCategories[i], GameManager.Instance.Chefs[i]));
                    else if (i < categoriesAmount - 1)
                        leaderboardCategories.Add(new TimeLeaderboardCategory(scoreCategories[i]));
                    else
                        leaderboardCategories.Add(new TotalLeaderboardCategory(scoreCategories[i], "Total", GameManager.Instance.CurrentRound - 1));
                }
            }
            else
            {
                roundLabel.text = "Pontuação Final";
                categoriesAmount = GameManager.Instance.TotalRounds;
                EnableCategories(categoriesAmount);
                for (int i = 0; i < categoriesAmount; i++)
                {
                    leaderboardCategories.Add(new TotalLeaderboardCategory(scoreCategories[i], $"Round {NumberToRoman(i + 1)}", i));
                }
            }
            for (int i = 0; i < playerScoreItems.Length; i++)
            {
                playerScoreItems[i].gameObject.SetActive(i < PlayerNetworkManager.Instance.PlayersCount);
                if (i < PlayerNetworkManager.Instance.PlayersCount)
                {
                    playerScoreItems[i].Initialize(categoriesAmount, PlayerNetworkManager.Instance.PlayersData[i], leaderboardCategories);
                }
            }
            for (int i = 0; i < readyIcons.Length; i++)
            {
                readyIcons[i].gameObject.SetActive(false);
                notReadyIcons[i].transform.parent.gameObject.SetActive(i < PlayerNetworkManager.Instance.PlayersCount);
            }
        }

        void EnableCategories(int amount)
        {
            for (int i = 0; i < scoreCategories.Length; i++)
            {
                divisions[i].SetActive(i < amount);
                scoreCategories[i].SetActive(i < amount);
            }
        }
        public static string NumberToRoman(int number)
        {
            if (number >= 5) return "V" + NumberToRoman(number - 5);
            if (number >= 4) return "IV" + NumberToRoman(number - 4);
            if (number >= 1) return "I" + NumberToRoman(number - 1);
            else return "";
        }
        //CHAMADA A CADA CHECK DO TOGGLE DE FIM DE JOGO
        public void SetReady()
        {
            playerIsReady = !playerIsReady;
            GameManager.Instance.ReadyPlayersServerRpc(PlayerNetworkManager.Instance.LocalNetworkID, playerIsReady);
        }
        public void UpdateReadyPlayers(int playerId, bool value)
        {
            readyIcons[playerId].gameObject.SetActive(value);
        }
        public void ResetInfo()
        {
            playerIsReady = false;
        }
        #region OLD


        //public List<KeyValuePair<int, float>> GetSortedPlayerScores()
        //{
        //    //NetworkManagerUI.Instance.finalResult.SetActive(true);
        //    foreach (var item in ScoreManager.Instance.playerScores)
        //    {
        //        FinalScores[item.Key] = item.Value;
        //    }
        //    //foreach (var item in EliminationPlayer.Instance.ElimPlayers)
        //    //{
        //    //    FinalScores[item.Key] = item.Value;
        //    //}
        //    var orderedPlayers = FinalScores.OrderByDescending(player => player.Value).ToList();
        //    //GameManager.Instance.numberPlayer = PlayerNetworkManager.Instance.GetPlayer.Count;

        //    return orderedPlayers;
        //}

        #endregion

        #region Classes
        [Serializable]
        public class ScoreCategory
        {
            public TextMeshProUGUI scoreCategoryText;
            public Image scoreCategoryImage;

            public void SetActive(bool value)
            {
                scoreCategoryText.transform.parent.gameObject.SetActive(value);
                scoreCategoryImage.gameObject.SetActive(value);
            }
            public void SetActive(bool textValue, bool imageValue)
            {
                scoreCategoryText.transform.parent.gameObject.SetActive(textValue);
                scoreCategoryImage.gameObject.SetActive(imageValue);
            }
        }

        [Serializable]
        internal abstract class LeaderboardCategory
        {
            protected TextMeshProUGUI categoryName;
            protected Image categorySprite;

            public LeaderboardCategory(ScoreCategory scoreCategory)
            {
                this.categoryName = scoreCategory.scoreCategoryText;
                this.categorySprite = scoreCategory.scoreCategoryImage;
            }

            public abstract float GetPlayerScore(int playerIndex);
        }

        class ChefLeaderboardCategory : LeaderboardCategory
        {
            ChefSO chef;
            public ChefLeaderboardCategory(ScoreCategory scoreCategory, ChefSO chef) : base(scoreCategory)
            {
                categoryName.text = chef.name;
                categorySprite.sprite = chef.sprite;
                this.chef = chef;
            }

            public override float GetPlayerScore(int playerIndex)
            {
                var playerScore = ScoreManager.Instance.PlayerScores[GameManager.Instance.CurrentRound - 1][playerIndex];
                if (playerScore.chefScores.ContainsKey(chef))
                    return playerScore.chefScores[chef];
                else
                    return 0;
            }
        }
        class TimeLeaderboardCategory : LeaderboardCategory
        {
            public TimeLeaderboardCategory(ScoreCategory scoreCategory) : base(scoreCategory)
            {
                categoryName.text = "Time";
                categorySprite.sprite = Instance.clockSprite;
            }

            public override float GetPlayerScore(int playerIndex)
            {
                return ScoreManager.Instance.PlayerScores[GameManager.Instance.CurrentRound - 1][playerIndex].timeScore;
            }
        }
        class TotalLeaderboardCategory : LeaderboardCategory
        {
            int targetRound;
            public TotalLeaderboardCategory(ScoreCategory scoreCategory, string categoryName, int targetRound) : base(scoreCategory)
            {
                this.categoryName.text = categoryName;
                this.targetRound = targetRound;
                categorySprite.sprite = Instance.equalsSprite;
            }

            public override float GetPlayerScore(int playerIndex)
            {
                return ScoreManager.Instance.PlayerScores[targetRound][playerIndex].Total;
            }
        }
        #endregion
    }
}