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
    public List<FoodSO> itensUsed = new List<FoodSO>();

    public override void Pick(Player player)
    {
        if(isHandIngredient){
            this.GetComponent<NetworkObject>().TrySetParent(player.transform);
            this.GetComponent<NetworkObject>().transform.position = player.assetIngredient.transform.position;
        }else{
            player.GetComponentInChildren<Tool>().ingredients.Add(food);
        }
        player.isHand = true;
    }
}
