using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Tool : Interactable
{
    public ToolsSO tool;
    public bool isHandTool;

    public override void Pick(Player player)
    {
        player.isHand = true;
        player.tool = tool;
        player.StatusAssetServerRpc(true);
        player.ChangeMeshHandToolServerRpc();
    }

    public void DestroySelf(){
        if(isHandTool){
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
