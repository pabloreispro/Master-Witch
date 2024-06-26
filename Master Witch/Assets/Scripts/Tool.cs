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
                    GameObject objectSpawn = Instantiate(tool.prefab, player.boneBasket.position, player.boneBasket.rotation);
                    objectSpawn.GetComponent<Collider>().enabled = false;
                    // Obtém ou adiciona o componente NetworkObject
                    NetworkObject networkObject = objectSpawn.GetComponent<NetworkObject>();
                    

                    // Spawna o objeto na rede
                    networkObject.Spawn();
                    networkObject.TrySetParent(player.transform);

                    // Adiciona ou obtém o componente FollowTransform para seguir o boneBasket
                    FollowTransform followTransform = objectSpawn.GetComponent<FollowTransform>();
                    if (followTransform == null)
                    {
                        followTransform = objectSpawn.AddComponent<FollowTransform>();
                    }
                    followTransform.targetTransform = player.boneBasket;

                    

                    // Configurações adicionais do jogador
                    player.isHandBasket = true;
                    player.isHand = true;
                    player.ChangeState(PlayerState.Interact);
                    
                    
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