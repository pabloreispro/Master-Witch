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
        if(!isHandTool)
        {
            if(IsServer)
            {
                if(this.tool.benchType == BenchType.Basket)
                {
                    /*Debug.Log("Entrei no if");

                    var i = Instantiate(tool.prefab,player.boneBasket);
                    i.transform.localScale = Vector3.one;
                    i.transform.localRotation = Quaternion.identity;
                    i.transform.localPosition = Vector3.zero;*/
                    var objectSpawn = Instantiate(tool.prefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
                    objectSpawn.GetComponent<NetworkObject>().Spawn();
                    objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
                    player.isHandBasket = true;
                    player.isHand = true;
                    player.ChangeState(PlayerState.Interact);
                    //player.ChangeState(PlayerState.IdleBasket);
                    
                }
                else
                {

                    var objectSpawn = Instantiate(tool.prefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
                    objectSpawn.GetComponent<NetworkObject>().Spawn();
                    objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);

                    player.isHand = true;
                    player.ChangeState(PlayerState.Interact);
                    //player.ChangeState(PlayerState.IdleItem);
                }
                
            }
                
        }
        else{
            this.GetComponent<NetworkObject>().TrySetParent(player.transform);
            player.isHand = true;
            player.ChangeState(PlayerState.Interact);
            //player.ChangeState(PlayerState.IdleItem);
        }
        
        
        //player.StatusAssetServerRpc(true);
        //player.ChangeMeshHandToolServerRpc();
    }
}