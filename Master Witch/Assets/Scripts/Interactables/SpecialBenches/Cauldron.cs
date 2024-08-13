using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cauldron : Bench
{
    private ToolsSO toolInBench;

    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Interactable>();
        if(interact as Ingredient){
            endProgress = false;
            AddIngredient((interact as Ingredient).food);
            if(toolInBench!=null){
                progress();
            }
        }else{
            toolInBench = (interact as Tool).tool;
        }
        interact.DestroySelf();
    }
}
