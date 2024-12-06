using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WoodStock : Bench
{
    public ToolsSO food;
    
    public override void Pick(Player player)
    {
        if(player.isHand.Value == false )
        {
            if(IsServer){
                var objectSpawn = Instantiate(food.prefab, new Vector3(player.boneItem.transform.position.x, 1.0f, player.boneItem.transform.position.z), Quaternion.identity);
                objectSpawn.GetComponent<NetworkObject>().Spawn();
                objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform); 
                objectSpawn.GetComponent<Collider>().enabled = false;
                player.ChangeState(PlayerState.PickItem);
                player.SetItemHandClientRpc(objectSpawn);
                
            }
        }
    }
}
