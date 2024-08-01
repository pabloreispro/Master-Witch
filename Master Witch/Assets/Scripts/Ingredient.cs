using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Unity.Netcode;

public class Ingredient : Interactable
{
    public FoodSO food;
    public bool isHandIngredient;
    public TypeObject typeObject;
    public List<RecipeData> itensUsed = new List<RecipeData>();

    public override void Pick(Player player)
    {
        if(isHandIngredient){
            this.GetComponent<Collider>().enabled = false;
            this.GetComponent<NetworkObject>().TrySetParent(player.transform);
            this.GetComponent<NetworkObject>().transform.position = player.boneItem.transform.position;
            this.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
            player.SetItemHandClientRpc(gameObject);
        }else{
            player.GetComponentInChildren<Tool>().ingredients.Add(new RecipeData(food));
        }
        player.isHand.Value = true;
        player.ChangeState(PlayerState.Interact);
    }
}
