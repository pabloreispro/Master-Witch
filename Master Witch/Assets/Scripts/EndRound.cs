using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UI;
using UnityEngine.Rendering;
using System.Linq;
using Unity.VisualScripting;
using Network;


public class EndRound : SingletonNetwork<EndRound>
{
    public Dictionary<int, float> FinalScores = new Dictionary<int, float>();

    public bool finalGame;

    public void ReturnMarket(){
        GameManager.Instance.OnReturnMarket();
        StartCoroutine(TransitionController.Instance.TransitionMarketScene());
        
    }


    public List<KeyValuePair<int, float>> finishGame(){
        //NetworkManagerUI.Instance.finalResult.SetActive(true);
        foreach(var item in EliminationPlayer.Instance.scoresPlayers){
            FinalScores[item.Key] = item.Value;
        }
        foreach(var item in EliminationPlayer.Instance.ElimPlayers){
            FinalScores[item.Key] = item.Value;
        }
        var orderedPlayers = FinalScores.OrderByDescending(player => player.Value).ToList();
        GameManager.Instance.numberPlayer = PlayerNetworkManager.Instance.GetPlayer.Count;
       
        return orderedPlayers;
    }
    
    public void CanNextRound(){
        int activeToggle = 0;
        for(int i=0; i<NetworkManagerUI.Instance.playerFinalCheck.Length; i++){
            if(NetworkManagerUI.Instance.playerFinalCheck[i].isOn){
                activeToggle++;
            }
        }
        if(activeToggle == GameManager.Instance.numberPlayer){
            if(GameManager.Instance.numberPlayer>2){
                if(!finalGame){
                    ReturnMarket();
                }else{
                    GameManager.Instance.EndGame();
                }
            }else{
                if(!finalGame){
                    NetworkManagerUI.Instance.UpdadeScreenFinalClientRpc();
                }else{
                    GameManager.Instance.EndGame();
                }
            }
            //StartCoroutine(TimeCounter());
        }
    }
}
