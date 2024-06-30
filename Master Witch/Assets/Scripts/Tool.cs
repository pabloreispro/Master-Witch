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
    public TypeObject typeObject;
    public bool isHandTool;
    public List<RecipeData> ingredients = new List<RecipeData>();
    
    public List<FoodSO> foodList 
    {
        get
        {
            var list = new List<FoodSO>();
            foreach (var item in ingredients)
            {
                list.Add(item.TargetFood);
            }
            return list;
        }
    }
    public override void Pick(Player player)
    {
        if(!isHandTool)
        {
            if(IsServer)
            {
                if(this.tool.benchType == BenchType.Basket)
                {
                    player.isHandBasket.Value = true;
                    player.isHand.Value = true;
                    player.ChangeState(PlayerState.Interact);

                    GameObject objectSpawn = Instantiate(tool.prefab, player.boneBasket.position, player.boneBasket.rotation);
                    objectSpawn.GetComponent<Collider>().enabled = false;
                    
                    NetworkObject networkObject = objectSpawn.GetComponent<NetworkObject>();
                    networkObject.Spawn();
                    networkObject.TrySetParent(player.transform);

        
                    player.SetBasketHandClientRpc(objectSpawn);


                    
                      
                }
                else
                {
                    Debug.Log("euuu tenteeeei");
                    player.isHand.Value = true;
                    player.ChangeState(PlayerState.Interact);

                    var objectSpawn = Instantiate(tool.prefab, player.boneItem.position, player.boneItem.rotation);
                    objectSpawn.GetComponent<Collider>().enabled = false;

                    NetworkObject networkObject = objectSpawn.GetComponent<NetworkObject>();
                    networkObject.Spawn();
                    networkObject.TrySetParent(player.transform);

                    player.SetItemHandClientRpc(objectSpawn);

                }
                
            }
                
        }
        else{
            player.isHand.Value = true;
            player.ChangeState(PlayerState.Interact);

            var item = this.gameObject;
            item.GetComponent<Collider>().enabled = false;
            item.GetComponent<NetworkObject>().TrySetParent(player.transform);

            player.SetItemHandClientRpc(item);
        }
        
    }
}