using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashBin : Bench
{
    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Interactable>();
        interact.DestroySelf();  
        player.isHandBasket.Value = false;
    }
}
