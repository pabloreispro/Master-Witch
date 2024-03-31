using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Unity.Netcode;

public class Ingredient : Interactable
{
    public FoodSO food;
    public bool isHandIngredient;


    public override void Pick(Player player)
    {
        player.assetIngredient.SetActive(true);
        player.isHand = true;
        player.ingredient = food;
        player.interact = null;
        if(isHandIngredient){
            Debug.Log("Destruiu");
            DestroyServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc(){
        this.GetComponent<NetworkObject>().Despawn();
    }


}
