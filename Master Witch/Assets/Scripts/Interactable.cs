using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public abstract class Interactable : NetworkBehaviour
{

    public virtual void Drop(Player player){
        
    }
    public virtual void Pick(Player player){}
    [ServerRpc(RequireOwnership = false)]
    public void DropServerRpc(ulong playerID){
        DropClientRpc(playerID);
    }
    [ClientRpc]
    public void DropClientRpc(ulong playerID){
        foreach(Player player in FindObjectsOfType<Player>()){
            if(player.NetworkObjectId == playerID){
                Debug.Log("depositou");
                Drop(player);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickServerRpc(ulong playerID){
        Debug.Log("Pegou");
        PickClientRpc(playerID);
    }
    [ClientRpc]
    public void PickClientRpc(ulong playerID){
        
        foreach(Player player in FindObjectsOfType<Player>()){
            if(player.NetworkObjectId == playerID){
                Pick(player);
            }
        }
    }

}
