using System.Collections;
using System.Collections.Generic;
using Game.SO;
using Unity.Mathematics;
using Unity.Netcode;
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

    public void progress(){
        
    }
    
    public void AddIngredient(FoodSO ingredient){
        //assetBench.SetActive(true);
        ingredients.Add(ingredient);
        assetBench = ingredients[0].foodPrefab;
        progress();

    }

    public void OnEndProgress(){

    }

    public override void Drop(Player player)
    {
        AddIngredient(player.ingredient);
        SpawnObject();
        player.stateObject = false;
        player.isHandfull = false;
    }

    void SpawnObject(){
        Vector3 spawnPosition = new Vector3(this.transform.position.x, 0.9f, this.transform.position.z);
        var objectSpawn = Instantiate(assetBench, spawnPosition, Quaternion.identity);
        //objectSpawn.GetComponent<NetworkObject>().Spawn(true);
    }

}
