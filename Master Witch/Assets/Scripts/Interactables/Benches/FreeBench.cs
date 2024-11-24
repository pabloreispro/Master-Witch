using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FreeBench : Bench
{
    public override void Pick(Player player)
    {
        if(!player.isHand.Value){
            this.GetComponentInChildren<Ingredient>().gameObject.transform.position = player.boneItem.transform.position;
            this.GetComponentInChildren<Ingredient>().gameObject.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
            this.GetComponentInChildren<Ingredient>().GetComponent<NetworkObject>().TrySetParent(player.transform);               
            player.SetItemHandClientRpc(GetComponentInChildren<Ingredient>().gameObject);
        }
        Reset();
    }
    public override void Drop(Player player)
    {
        if(this.GetComponentInChildren<Interactable>() == null){
            var interact = player.GetComponentInChildren<Interactable>();
            PositionBench(interact);
        }
    }
}
