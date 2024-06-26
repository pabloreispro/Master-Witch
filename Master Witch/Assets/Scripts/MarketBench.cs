using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
public class MarketBench : Interactable
{
    public FoodSO food;
    public ToolsSO tool;
    
    public override void Pick(Player player)
    {
        if(player.isHand.Value == true && player.GetComponentInChildren<Tool>().tool == tool)
        {
           player.AddItemBasket(food); 
        }
    }

}
