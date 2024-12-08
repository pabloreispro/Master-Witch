using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Network;
using System;

public enum TypeObject{Ingredient, Tool}
public abstract class Interactable : NetworkBehaviour
{
    public Action<bool> action; //True if is Pick Action
    public virtual void Drop(Player player){}
    public virtual void Pick(Player player){}

    [ServerRpc(RequireOwnership = false)]
    public void DropServerRpc(ulong playerID){
        DropClientRpc(playerID);
    }
    [ClientRpc]
    public void DropClientRpc(ulong playerID){
        foreach(Player player in FindObjectsOfType<Player>()){
            if(player.NetworkObjectId == playerID){
                Drop(player);
                action?.Invoke(false);
            }
        }
        //Drop(PlayerNetworkManager.Instance.playerList[playerID]);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickServerRpc(ulong playerID){
        PickClientRpc(playerID);
    }
    [ClientRpc]
    public void PickClientRpc(ulong playerID){
        int i = 0;
        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.NetworkObjectId == playerID)
            {
                Pick(player);
                action?.Invoke(true);
            }
            i++;
        }
        //Pick(PlayerNetworkManager.Instance.playerList[playerID]);
    }

    public void DestroySelf(){
        DestroyServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    public void DestroyServerRpc(){
        this.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
        this.GetComponent<NetworkObject>().Despawn();
    }

}
