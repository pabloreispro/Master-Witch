using System.Collections;
using System.Collections.Generic;
using Game.SO;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public enum BenchType {Oven, Stove, Board}
public class Bench : Interactable
{
    int playerID;
    [SerializeField]
    List<FoodSO> ingredients = new List<FoodSO>();
    BenchType benchType;
    public GameObject assetBench;
    public Vector3 positionSpawn;
    public bool endProgress;

    public void progress(){
        SpawnObject();

    }
    
    public void AddIngredient(FoodSO ingredient){
        //assetBench.SetActive(true);
        ingredients.Add(ingredient);
        assetBench = ingredients[0].foodPrefab;
        progress();

    }

    public void OnEndProgress(){

    }

    public override void Pick(Player player)
    {
        if(endProgress){
            player.isHand = true;
            if(ingredients.Count>0){
                player.ingredient = ingredients[0];
                ingredients.Clear();
            }
            
        }
    }

    public override void Drop(Player player)
    {
        AddIngredient(player.ingredient);
        player.ResetStatus(false);
    }

    void SpawnObject(){
        Vector3 spawnPosition = new Vector3(this.transform.position.x, 1f, this.transform.position.z);
        Instantiate(assetBench, spawnPosition, Quaternion.identity);
    }
}
