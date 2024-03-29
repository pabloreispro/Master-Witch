using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Unity.Netcode;

public class Ingredient : Interactable
{
    public float progress;
    public FoodSO food;
    public bool valueAsset;

    public override void Pick(Player player)
    {
        
        player.stateObject = true;
        player.isHandfull = true;
        player.ingredient = food;
    }
    
}
