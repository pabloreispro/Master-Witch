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

namespace UI.Leaderboard
{
    public class LeaderboardManager : Singleton<LeaderboardManager>
    {
        const int ADDITIONAL_CATEGORIES = 2;
        public Dictionary<int, float> FinalScores = new Dictionary<int, float>();

        [SerializeField] TextMeshProUGUI roundLabel;
        [SerializeField] GameObject[] divisions;
        [SerializeField] ScoreCategory[] scoreCategories;
        [SerializeField] PlayerScoreItem[] playerScoreItems;
        [SerializeField] protected Sprite clockSprite;
        [SerializeField] protected Sprite equalsSprite;
        List<LeaderboardCategory> leaderboardCategories = new List<LeaderboardCategory>();
        public void EnableRoundLeaderboard()
        {
            roundLabel.text = "Round " + NumberToRoman(GameManager.Instance.CurrentRound);
            int categoriesAmount = GameManager.Instance.Chefs.Count + ADDITIONAL_CATEGORIES;
            EnableCategories(categoriesAmount);
            for (int i = 0; i < categoriesAmount; i++)
            {
                if (i < GameManager.Instance.Chefs.Count)
                    leaderboardCategories.Add(new ChefLeaderboardCategory(scoreCategories[i], GameManager.Instance.Chefs[i]));
                else if(i < categoriesAmount - 1)
                    leaderboardCategories.Add(new TimeLeaderboardCategory(scoreCategories[i]));
                else
                    leaderboardCategories.Add(new TotalLeaderboardCategory(scoreCategories[i]));
            }
            for (int i = 0; i < playerScoreItems.Length; i++)
            {
                playerScoreItems[i].gameObject.SetActive(i < PlayerNetworkManager.Instance.PlayersCount);
                if (i < PlayerNetworkManager.Instance.PlayersCount)
                {
                    playerScoreItems[i].Initialize(categoriesAmount, PlayerNetworkManager.Instance.PlayersData[i], leaderboardCategories);
                }
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

        #region OLD
        public void ReturnMarket()
        {
            GameManager.Instance.OnReturnMarket();
            //NewCamController.Instance.IntroClient();
            GameInterfaceManager.Instance.clock.SetActive(true);
            GameInterfaceManager.Instance.recipeSteps.SetActive(true);
            GameManager.Instance.InitializeGame(false);
            StartCoroutine(TransitionController.Instance.TransitionMarketScene());
        }


        public List<KeyValuePair<int, float>> GetSortedPlayerScores()
        {
            //NetworkManagerUI.Instance.finalResult.SetActive(true);
            foreach (var item in ScoreManager.Instance.playerScores)
            {
                FinalScores[item.Key] = item.Value;
            }
            //foreach (var item in EliminationPlayer.Instance.ElimPlayers)
            //{
            //    FinalScores[item.Key] = item.Value;
            //}
            var orderedPlayers = FinalScores.OrderByDescending(player => player.Value).ToList();
            //GameManager.Instance.numberPlayer = PlayerNetworkManager.Instance.GetPlayer.Count;

            return orderedPlayers;
        }

        //CHAMADA A CADA CHECK DO TOGGLE DE FIM DE JOGO
        public void CanNextRound()
        {
            int activeToggle = 0;
            //for (int i = 0; i < GameInterfaceManager.Instance.playerFinalCheck.Length; i++)
            //{
            //    if (GameInterfaceManager.Instance.playerFinalCheck[i].Toggle.isOn)
            //    {
            //        activeToggle++;
            //    }
            //}
            //if(activeToggle == GameManager.Instance.numberPlayer){
            //    if(GameManager.Instance.numberPlayer>2){
            if (activeToggle >= PlayerNetworkManager.Instance.PlayersData.Length)
            {
                //if (!finalGame)
                //{
                //    //if (PlayerNetworkManager.Instance.PlayersData.Length > 2)
                //    if (GameManager.Instance.CurrentRound < GameManager.Instance.TotalRounds)
                //        ReturnMarket();
                //    else
                //        GameInterfaceManager.Instance.UpdadeScreenFinalClientRpc();
                //}
                //else
                //{
                //    GameManager.Instance.EndGame();
                //}
                //if (PlayerNetworkManager.Instance.PlayersData.Length > 2)
                if (GameManager.Instance.CurrentRound < GameManager.Instance.TotalRounds)
                    ReturnMarket();
                else
                    GameManager.Instance.EndGame();
            }
            //StartCoroutine(TimeCounter());
        }
        #endregion

        [Serializable]
        public class ScoreCategory
        {
            public TextMeshProUGUI scoreCategoryText;
            public Image scoreCategoryImage;

            public void SetActive(bool value)
            {
                scoreCategoryText.gameObject.SetActive(value);
                scoreCategoryImage.gameObject.SetActive(value);
            }
            public void SetActive(bool textValue, bool imageValue)
            {
                scoreCategoryText.gameObject.SetActive(textValue);
                scoreCategoryImage.gameObject.SetActive(imageValue);
            }
        }

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
                ScoreManager.Instance.playerScores[playerIndex] += UnityEngine.Random.Range(0, 100); 
                return ScoreManager.Instance.playerScores[playerIndex];
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
                return 10;
            }
        }
        class TotalLeaderboardCategory : LeaderboardCategory
        {
            public TotalLeaderboardCategory(ScoreCategory scoreCategory) : base(scoreCategory)
            {
                categoryName.text = "Total";
                categorySprite.sprite = Instance.equalsSprite;
            }

            public override float GetPlayerScore(int playerIndex)
            {
                return ScoreManager.Instance.playerScores[playerIndex] + 10;
            }
        }
    }
}