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
    public bool nextRound;


    public void ReturnMarket(){
        
        NetworkManagerUI.Instance.finalPanel.SetActive(false);
        EliminationPlayer.Instance.PlayerElimination();
        GameManager.Instance.Reset();
        StartCoroutine(TransitionController.Instance.TransitionMarketScene());
    }


    public void finishGame(){
        //NetworkManagerUI.Instance.finalResult.SetActive(true);
        foreach(var item in EliminationPlayer.Instance.scoresPlayers){
            FinalScores[item.Key] = item.Value;
        }
        foreach(var item in EliminationPlayer.Instance.ElimPlayers){
            FinalScores[item.Key] = item.Value;
        }
        var orderedPlayers = FinalScores.OrderByDescending(player => player.Value).ToList();
        GameManager.Instance.numberPlayer = PlayerNetworkManager.Instance.GetPlayer.Count;
        GameManager.Instance.Reset();
        NetworkManagerUI.Instance.UpdateFinalResult(orderedPlayers);
    }
    public void CanNextRound(){
        int activeToggle = 0;
        for(int i=0; i<NetworkManagerUI.Instance.playerFinalCheck.Length; i++){
            if(NetworkManagerUI.Instance.playerFinalCheck[i].isOn){
                activeToggle++;
            }
        }
        if(activeToggle == GameManager.Instance.numberPlayer){
            if(GameManager.Instance.numberPlayer>1){
                ReturnMarket();
            }else{
                finishGame();
            }
            //StartCoroutine(TimeCounter());
        }
    }
}
