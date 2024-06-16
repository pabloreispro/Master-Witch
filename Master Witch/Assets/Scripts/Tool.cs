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
        if(!isHandTool){
            if(IsServer){
                if(this.tool.benchType == BenchType.Basket)
                {
                    var objectSpawn = Instantiate(tool.prefab, player.boneBasket);
                    objectSpawn.transform.localPosition = Vector3.zero;
                    objectSpawn.transform.localRotation = Quaternion.identity;
                    objectSpawn.GetComponent<NetworkObject>().Spawn();
                    objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
                    player.isHandBasket = true;
                    player.ChangeState(PlayerState.IdleBasket);
                }
                else
                {
                    var objectSpawn = Instantiate(tool.prefab, player.boneItem);
                    objectSpawn.transform.localPosition = Vector3.zero;
                    objectSpawn.transform.localRotation = Quaternion.identity;
                    objectSpawn.GetComponent<NetworkObject>().Spawn();
                    objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.boneItem);
                    player.ChangeState(PlayerState.IdleItem);
                }
                
            }
            
            
        }
        else{
            this.GetComponent<NetworkObject>().TrySetParent(player.boneItem);
        }
        player.isHand = true;
        
        //player.StatusAssetServerRpc(true);
        //player.ChangeMeshHandToolServerRpc();
    }
}
