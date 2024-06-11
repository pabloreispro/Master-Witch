using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UI;

public class EndRound : SingletonNetwork<EndRound>
{
    NetworkVariable<int> timerCount = new NetworkVariable<int>();

    public override void OnNetworkSpawn(){
        timerCount.Value = 5;
    }

    [ServerRpc(RequireOwnership =false)]
    public void ReturnMarketServerRpc(){
        SceneManager.Instance.ChangeSceneServerRpc(true ,false);
        SceneManager.Instance.RepositionPlayerServerRpc();
        ReturnMarketClientRpc();
        
    }

    [ClientRpc]
    public void ReturnMarketClientRpc(){
        NetworkManagerUI.Instance.finalPanel.SetActive(false);
        EliminationPlayer.Instance.ElimPlayer();
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
            //StartCoroutine(TimeCounter());
        }
    }
    IEnumerator TimeCounter()
    {
        while(timerCount.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            timerCount.Value -= 1;
        }
    }
}
