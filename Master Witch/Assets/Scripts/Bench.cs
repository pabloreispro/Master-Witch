using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BenchType {Oven, Stove, Board}
public class Bench : Interactable
{
    int playerID;
    List<Ingredient> ingredients = new List<Ingredient>();
    BenchType benchType;

    public void progress(){

    }
    public void AddIngredient(){
        
    }

    public void OnEndProgress(){

    }
}
