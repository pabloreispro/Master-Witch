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

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L)){
            //EndRound.Instance.CanNextRoundServerRpc();
        }
    }


    public void AddScoresPlayers(){
        foreach(var item in PlayerNetworkManager.Instance.GetID){
            scoresPlayers.Add(Convert.ToInt32(item.Value), 0);
            //NetworkManagerUI.Instance.UpdatePlayerScore(Convert.ToInt32(item.Value), 0);
        }
    }
    public void GetPlayerScore(int playerID, RecipeData recipe)
    {
        float score = 0;
        foreach (var chef in GameManager.Instance.Chefs)
        {
            var chefScore = chef.ReviewRecipe(recipe);
            Debug.Log($"Total score of {recipe.TargetFood.name} by {chef.name} is {chefScore}");
            score += chefScore;
        }
        score /= GameManager.Instance.Chefs.Count;
        Debug.Log($"Total score for Player {playerID} is {score}");
        UpdadeScoresPlayers(playerID, score);
    }

    public void UpdadeScoresPlayers(int playerID, float score){
        scoresPlayers[playerID] = score;
    }
 
    public void PlayerElimination(){
        var player = scoresPlayers.Aggregate((l,r) => l.Value<r.Value ? l : r); 
        Debug.Log("Player Eliminado no server e: "+player.Key);
        OnPlayerEliminatedClientRpc(player.Key);
        ElimPlayers.Add(player.Key, player.Value);
        scoresPlayers.Remove(player.Key);
        GameManager.Instance.numberPlayer--;
    }

    [ClientRpc]
    public void OnPlayerEliminatedClientRpc(int playerID){
        Debug.Log("Player eliminado é: "+ playerID);
        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.id == playerID)
            {
                player.GetComponent<NetworkObject>().gameObject.SetActive(false);
            }
        }
        //PlayerNetworkManager.Instance.GetPlayerByIndex(playerID).gameObject.SetActive(false);
    }

    public void Reset(){
        scoresPlayers.Clear();
        ElimPlayers.Clear();
    }
}
