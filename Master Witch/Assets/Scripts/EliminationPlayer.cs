using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using Unity.Netcode;
using UnityEngine;
using UI;

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

            foreach(var item in scoresPlayers){
                Debug.Log("Player scores id: " + item.Key +" score: "+ item.Value);
            }
            foreach(var item in ElimPlayers){
                Debug.Log("Player eliminados id: " + item.Key +" score: "+ item.Value);
            }
        }
    }


    public void AddScoresPlayers(){
        foreach(var item in PlayerNetworkManager.Instance.GetID){
            scoresPlayers.Add(Convert.ToInt32(item.Value), 0);
            NetworkManagerUI.Instance.UpdatePlayerScore(Convert.ToInt32(item.Value), 0);
        }
    }

    public void UpdadeScoresPlayers(int playerID, float score){
        scoresPlayers[playerID] = score;
        NetworkManagerUI.Instance.UpdatePlayerScore(playerID, score);
    }
    public void PlayerElimination(){
        var player = scoresPlayers.Aggregate((l,r) => l.Value<r.Value ? l : r); 
        PlayerNetworkManager.Instance.GetPlayerByIndex(player.Key).gameObject.SetActive(false);
        ElimPlayers.Add(player.Key, player.Value);
        scoresPlayers.Remove(player.Key);
        GameManager.Instance.numberPlayer--;
    }

    public void Reset(){
        scoresPlayers.Clear();
        ElimPlayers.Clear();
    }
}
