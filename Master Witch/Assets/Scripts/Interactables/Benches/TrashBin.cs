using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TrashBin : Bench
{
    public AudioSource sfx;

    [ClientRpc]
    public void EnableSFXClientRpc(){
        sfx.Play();
    }
    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Interactable>();
        interact.DestroySelf();  
        player.isHandBasket.Value = false;
        EnableSFXClientRpc();
    }
}
