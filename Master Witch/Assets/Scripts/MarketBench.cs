using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
public class MarketBench : Interactable
{
    public FoodSO food;
    public bool getBasket;
    public override void Pick(Player player)
    {
        if(getBasket == true)
        {
            player.hasBasket = true;
        }
        else if(player.hasBasket == true && getBasket != true)
        {
           player.AddItemBasket(food); 
        }
        
        
    }

}
