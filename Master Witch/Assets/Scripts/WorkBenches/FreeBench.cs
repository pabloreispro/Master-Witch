using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FreeBench : Bench
{
    public override void Pick(Player player)
    {
        
        this.GetComponentInChildren<Ingredient>().gameObject.transform.position = player.boneItem.transform.position;
        this.GetComponentInChildren<Ingredient>().gameObject.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
        this.GetComponentInChildren<Ingredient>().GetComponent<NetworkObject>().TrySetParent(player.transform);               
        player.SetItemHandClientRpc(GetComponentInChildren<Ingredient>().gameObject);
        
        //toolInBench.transform.position = player.boneItem.transform.position;
        //toolInBench.GetComponentInChildren<NetworkObject>().TrySetParent(player.transform);
        //toolInBench.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
        //player.SetItemHandClientRpc(toolInBench.gameObject);
        //toolInBench=null;
        
        Reset();
    }
    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Interactable>();
        PositionBench(interact);
        player.isHand.Value = false;
        
    }
}
