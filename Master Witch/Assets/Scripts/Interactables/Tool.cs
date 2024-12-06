using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Game.SO;
using System;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class Tool : Interactable
{
    public ToolsSO tool;
    private void Start() {
         this.GetComponent<Collider>().enabled = false;
    }
    
    public override void Pick(Player player)
    {
        
        this.GetComponent<Collider>().enabled = false;
        this.GetComponent<NetworkObject>().TrySetParent(player.transform);
        this.GetComponent<NetworkObject>().transform.position = player.boneItem.transform.position;
        this.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
        player.ChangeState(PlayerState.PickItem);
        player.SetItemHandClientRpc(gameObject);
        
    }
    
}