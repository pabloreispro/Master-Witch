using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using Unity.Netcode;
using UnityEngine;
using UI;
using Game.SO;
using Game.SceneGame;

public class ScoreManager : Singleton<ScoreManager>
{
    /// <summary>
    /// Array pra cada round, com um array de pontuações de cada player
    /// </summary>
    [SerializeField] PlayerScore[][] playerScores;
    [SerializeField] List<PlayerScore> debug = new List<PlayerScore>();
    public PlayerScore[][] PlayerScores => playerScores;
    //public Dictionary<int , float> ElimPlayers = new Dictionary<int, float>();
    
    // Start is called before the first frame update
    void Start()
    {
        playerScores = new PlayerScore[GameManager.Instance.TotalRounds][];
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i] = new PlayerScore[PlayerNetworkManager.Instance.PlayersCount];
            for (int j = 0; j < playerScores[i].Length; j++)
            {
                playerScores[i][j] = new PlayerScore();
                debug.Add(playerScores[i][j]);
            }
        }
    }

    public void SetPlayerScore(int playerID, RecipeData recipe)
    {
        foreach (var chef in GameManager.Instance.Chefs)
        {
            float score = ChefSO.BASE_RECIPE_SCORE + chef.ReviewRecipe(recipe);
            playerScores[GameManager.Instance.CurrentRound - 1][playerID].chefScores[chef] = score;
            Debug.Log($"Total score of {recipe.TargetFood.name} by {chef.name} is {score}");
        }
        var matchTime = GameManager.Instance.matchStartTime + SceneManager.Instance.TIMER_MAIN / 2;
        var startTime = Mathf.Max(Time.time - matchTime, 0);
        playerScores[GameManager.Instance.CurrentRound - 1][playerID].timeScore = Mathf.Lerp(30, 0, startTime / (SceneManager.Instance.TIMER_MAIN / 2));

        Debug.Log($"score {Mathf.Lerp(30, 0, startTime / SceneManager.Instance.TIMER_MAIN)} Time {Time.time} Start{GameManager.Instance.matchStartTime} match {matchTime}");
        Debug.Log($"Total score for Player {playerID} is {playerScores[GameManager.Instance.CurrentRound - 1][playerID].Total}");
    }

    //public int PlayerElimination(){
    //    var player = scoresPlayers.Aggregate((l,r) => l.Value<r.Value ? l : r); 
    //    Debug.Log("Player Eliminado no server e: "+player.Key);
    //    ElimPlayers.Add(player.Key, player.Value);
    //    scoresPlayers.Remove(player.Key);
    //    return player.Key;
    //}

    [Serializable]
    public class PlayerScore
    {
        public int playerIndex;
        public Dictionary<ChefSO, float> chefScores = new Dictionary<ChefSO, float>();
        public bool[] hasLovedIngredient;
        public float timeScore;
        public float Total => chefScores.Values.Sum() + timeScore;
    }
}
