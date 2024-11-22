using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using Unity.Netcode;
using UnityEngine;
using UI;
using Game.SO;

public class ScoreManager : Singleton<ScoreManager>
{
    public Dictionary<int, float> playerScores = new Dictionary<int, float>();
    //public Dictionary<int , float> ElimPlayers = new Dictionary<int, float>();
    


    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void AddScoresPlayers(){
        //foreach(var item in PlayerNetworkManager.Instance.GetID){
        //    scoresPlayers.Add(Convert.ToInt32(item.Value), 0);
        for (int i = 0; i < PlayerNetworkManager.Instance.PlayersData.Length; i++)
        {
            playerScores.Add(PlayerNetworkManager.Instance.PlayersData[i].PlayerIndex, 0);
        }
    }
    public void GetPlayerScore(int playerID, RecipeData recipe)
    {
        float score = ChefSO.BASE_RECIPE_SCORE;
        foreach (var chef in GameManager.Instance.Chefs)
        {
            var chefScore = chef.ReviewRecipe(recipe);
            Debug.Log($"Total score of {recipe.TargetFood.name} by {chef.name} is {chefScore}");
            score += chefScore;
        }
        var matchTime = GameManager.Instance.matchStartTime + SceneManager.TIMER_MAIN / 2;
        var startTime = Mathf.Max(Time.time - matchTime, 0);
        score += Mathf.Lerp(30, 0, startTime / (SceneManager.TIMER_MAIN / 2));

        Debug.Log($"score {Mathf.Lerp(30, 0, startTime / SceneManager.TIMER_MAIN)} Time {Time.time} Start{GameManager.Instance.matchStartTime} match {matchTime}");
        Debug.Log($"Total score for Player {playerID} is {score}");
        UpdatePlayerScores(playerID, score);
    }

    public void UpdatePlayerScores(int playerID, float score){
        playerScores[playerID] += score;
    }

    //public int PlayerElimination(){
    //    var player = scoresPlayers.Aggregate((l,r) => l.Value<r.Value ? l : r); 
    //    Debug.Log("Player Eliminado no server e: "+player.Key);
    //    ElimPlayers.Add(player.Key, player.Value);
    //    scoresPlayers.Remove(player.Key);
    //    return player.Key;
    //}

    

    public void Reset(){
        playerScores.Clear();
        //ElimPlayers.Clear();
    }
}
