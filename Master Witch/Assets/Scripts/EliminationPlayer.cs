using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using Unity.Netcode;
using UnityEngine;
using UI;
using Game.SO;

public class EliminationPlayer : Singleton<EliminationPlayer>
{
    public Dictionary<int, float> scoresPlayers = new Dictionary<int, float>();
    public Dictionary<int , float> ElimPlayers = new Dictionary<int, float>();
    


    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void AddScoresPlayers(){
        foreach(var item in PlayerNetworkManager.Instance.GetID){
            scoresPlayers.Add(Convert.ToInt32(item.Value), 0);
            //NetworkManagerUI.Instance.UpdatePlayerScore(Convert.ToInt32(item.Value), 0);
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
        score /= GameManager.Instance.Chefs.Count;
        var matchTime = GameManager.Instance.matchStartTime + SceneManager.TIMER_MAIN / 2;
        var startTime = Mathf.Max(Time.time - matchTime, 0);
        score += Mathf.Lerp(30, 0, startTime / (SceneManager.TIMER_MAIN / 2));

        Debug.Log($"score {Mathf.Lerp(30, 0, startTime / SceneManager.TIMER_MAIN)} Time {Time.time} Start{GameManager.Instance.matchStartTime} match {matchTime}");
        Debug.Log($"Total score for Player {playerID} is {score}");
        UpdadeScoresPlayers(playerID, score);
    }

    public void UpdadeScoresPlayers(int playerID, float score){
        scoresPlayers[playerID] += score;
    }

    public int PlayerElimination(){
        var player = scoresPlayers.Aggregate((l,r) => l.Value<r.Value ? l : r); 
        Debug.Log("Player Eliminado no server e: "+player.Key);
        ElimPlayers.Add(player.Key, player.Value);
        scoresPlayers.Remove(player.Key);
        return player.Key;
    }

    

    public void Reset(){
        scoresPlayers.Clear();
        ElimPlayers.Clear();
    }
}
