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
        Debug.Log("entrei");
        if(player.isHand == true && player.tool == tool)
        {
           player.AddItemBasket(food); 
        }
        
        
    }

}
