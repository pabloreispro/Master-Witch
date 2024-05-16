using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Game.SO;
using System;

public class Tool : Interactable
{
    public ToolsSO tool;
    public TypeObject typeObject;
    public bool isHandTool;
    public RecipeSO foodFinish;
    public List<FoodSO> ingredients = new List<FoodSO>();
    

    public override void Pick(Player player)
    {
        if(!isHandTool){
            
            var objectSpawn = Instantiate(tool.prefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
            objectSpawn.GetComponent<NetworkObject>().Spawn();
            objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
            objectSpawn.transform.position = player.assetIngredient.transform.position;
            

            if(this.tool.benchType == BenchType.Basket)
            {
                player.isHandBasket = true;
            }
            
        }else{
            this.GetComponent<NetworkObject>().TrySetParent(player.transform);
        }
        player.isHand = true;
        //player.StatusAssetServerRpc(true);
        //player.ChangeMeshHandToolServerRpc();
    }
}
