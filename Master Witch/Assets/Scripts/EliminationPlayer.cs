using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using Unity.Netcode;
using UnityEngine;

public class EliminationPlayer : Singleton<EliminationPlayer>
{
    Dictionary<int, float> scoresPlayers = new Dictionary<int, float>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L)){
            foreach(var item in scoresPlayers){
                Debug.Log("Player id: " + item.Key +" score: "+ item.Value);
            }
        }
    }


    public void AddScoresPlayers(){
        foreach(var item in PlayerNetworkManager.Instance.GetID){
            scoresPlayers.Add(Convert.ToInt32(item.Value), 0);
        }
    }

    public void UpdadeScoresPlayers(int playerID, float score){
        scoresPlayers[playerID] = score;
        foreach(var item in scoresPlayers){
            Debug.Log("Player id: " + item.Key +" score: "+ item.Value);
        }
    }
    [ServerRpc]
    public void ElimPlayerServerRpc(){
        ElimPlayerClientRpc();
    }
    [ClientRpc]
    public void ElimPlayerClientRpc(){
        var player = scoresPlayers.Aggregate((l,r) => l.Value<r.Value ? l : r).Key; 
        Debug.Log("Player eliminado é: "+player);
    }
}
