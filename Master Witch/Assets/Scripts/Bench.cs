using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BenchType {Oven, Stove, Board}
public class Bench : Interactable
{
    int playerID;
    List<Ingredient> ingredients = new List<Ingredient>();
    BenchType benchType;
    public GameObject assetBench;

    public void progress(){

    }
    public void AddIngredient(){
        assetBench.SetActive(true);
    }

    public void OnEndProgress(){

    }

    public override void Drop(Player player)
    {
        AddIngredient();
        player.assetIngredient.SetActive(false);
    }
}
