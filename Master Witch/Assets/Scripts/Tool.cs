using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Game.SO;

public class Tool : Interactable
{
    public ToolsSO tool;
    public bool isHandTool;
    public FoodSO foodFinish;
    public List<FoodSO> ingredients = new List<FoodSO>();

    public override void Pick(Player player)
    {
        if(!isHandTool){
            if(IsServer){
                var objectSpawn = Instantiate(tool.prefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
                objectSpawn.GetComponent<NetworkObject>().Spawn();
                objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
            }
        }else{
            this.GetComponent<NetworkObject>().TrySetParent(player.transform);
        }
        player.isHand = true;
        player.isIngredient = false;
        //player.StatusAssetServerRpc(true);
        //player.ChangeMeshHandToolServerRpc();
    }

}
