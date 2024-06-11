using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UI;

public class EndRound : SingletonNetwork<EndRound>
{
    [ServerRpc(RequireOwnership =false)]
    public void ReturnMarketServerRpc(){
        SceneManager.Instance.ChangeSceneServerRpc(true ,false);
        SceneManager.Instance.RepositionPlayerServerRpc();
        ReturnMarketClientRpc();
    }

    [ClientRpc]
    public void ReturnMarketClientRpc(){
        NetworkManagerUI.Instance.finalPanel.SetActive(false);
    }
    public void CanNextRound(){
        int activeToggle = 0;
        for(int i=0; i<NetworkManagerUI.Instance.playerFinalCheck.Length; i++){
            if(NetworkManagerUI.Instance.playerFinalCheck[i].isOn){
                activeToggle++;
            }
        }
        if(activeToggle == GameManager.Instance.numberPlayer){
            ReturnMarketServerRpc();
        }
    }
}
