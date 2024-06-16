using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UI;
using Network;

public class ToggleState : SingletonNetwork<ToggleState>
{
    public Toggle toggle;
    public int id;

    void Start()
    {
        
        toggle = gameObject.GetComponent<Toggle>();
        if (toggle != null)
        {
            if(PlayerNetworkManager.Instance.GetPlayerByIndex(id).NetworkObjectId == NetworkManager.SpawnManager.GetLocalPlayerObject().NetworkObjectId){
                toggle.onValueChanged.AddListener(OnToggleValueChanged);
                toggle.interactable = true;
            }else{
                toggle.interactable = false;
            }
        }
    }

    void Update(){
        if(NetworkManagerUI.Instance.finalPanel.activeSelf){
            foreach(var item in EliminationPlayer.Instance.scoresPlayers){
                NetworkManagerUI.Instance.UpdatePlayerScoreServerRpc(item.Key, item.Value);
            }
        }
    }

    void OnToggleValueChanged(bool isOn)
    {        
        GameManager.Instance.ReadyPlayersServerRpc(id, isOn);
    }

}
