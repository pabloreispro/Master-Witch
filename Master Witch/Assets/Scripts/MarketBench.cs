using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
public class MarketBench : Interactable
{
    public FoodSO food;
    
    public override void Pick(Player player)
    {
        player.AddItemBasket(food);
    }

}
