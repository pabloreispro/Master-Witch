using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Unity.Netcode;
public class MarketBench : Interactable
{
    public FoodSO food;
    public ToolsSO tool;
    
    public override void Pick(Player player)
    {
        if(player.isHand.Value == false )
        {
            //player.AddItemBasket(food); 
            var objectSpawn = Instantiate(food.foodPrefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
            objectSpawn.GetComponent<NetworkObject>().Spawn();
            objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform); 
            objectSpawn.GetComponent<Collider>().enabled = false;
            player.SetItemHandClientRpc(objectSpawn);
            player.ChangeState(PlayerState.Interact);
            player.isHand.Value = true; 
        }
    }

}
