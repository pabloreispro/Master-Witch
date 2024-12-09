using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FreeBench : Bench
{
    void FixedUpdate(){

    }
    public override void Pick(Player player)
    {
        if (!player.isHand.Value){
            var ingredient = objectInBench.GetComponent<Ingredient>();
            ingredient.gameObject.transform.position = player.boneItem.transform.position;
            ingredient.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
            player.SetItemHandClientRpc(ingredient.gameObject);
            ingredient.GetComponent<NetworkObject>().TrySetParent(player.transform);               
            objectInBench = null;
            ingredients.Clear();
        }
        Reset();
    }
    public override void Drop(Player player)
    {
        if(objectInBench == null){
            var interact = player.GetComponentInChildren<Interactable>();
            objectInBench = interact.gameObject;
            PositionBench(interact);
            ingredients.Add(new RecipeData((interact as Ingredient).food));
        }
    }

    
}
