using System.Collections;
using System.Collections.Generic;
using Game.SO;
using UnityEngine;

public enum BenchType {Oven, Stove, Board}
public class Bench : Interactable
{
    int playerID;
    [SerializeField]
    List<FoodSO> ingredients = new List<FoodSO>();
    BenchType benchType;
    public GameObject assetBench;

    public void progress(){

    }
    public void AddIngredient(FoodSO ingredient){
        assetBench.SetActive(true);
        ingredients.Add(ingredient);
    }

    public void OnEndProgress(){

    }

    public override void Drop(Player player)
    {
        AddIngredient(player.ingredient);
        player.stateObject = false;
        player.isHandfull = false;
    }
}
