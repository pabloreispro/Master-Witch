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
        player.isHand = true;
        player.ingredient = food;
        player.interact = null;
        DestroySelf();
    }

    public void DestroySelf(){
        if(isHandIngredient){
            
            DestroyServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc(){
        Debug.Log("Destruiu");
        this.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
        this.GetComponent<NetworkObject>().Despawn();
    }


}
