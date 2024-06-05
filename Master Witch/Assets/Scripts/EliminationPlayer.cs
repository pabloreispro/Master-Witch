using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
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
        if(Input.GetKeyDown(KeyCode.Q)){
            ElimPlayer();
        }
    }

    public void AddScoresPlayers(int playerID, float score){
        scoresPlayers.Add(playerID, score);
    }

    public void ElimPlayer(){
        var player = scoresPlayers.Aggregate((l,r) => l.Value<r.Value ? l : r).Key;
        Debug.Log("Player q vai ser eliminado "+player);
        foreach(var item in scoresPlayers){
            Debug.Log("ID dos jogadores: "+item.Value+"Scores dos jogadores: "+item.Value);
        }
    }
}
