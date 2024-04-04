using System.Collections;
using System.Collections.Generic;
using Game.SO;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum BenchType {Oven, Stove, Board, Storage}
public class Bench : Interactable
{
    int playerID;
    [SerializeField]
    List<FoodSO> ingredients = new List<FoodSO>();
    public BenchType benchType;

    public GameObject[] assetBench;
    public bool isSpawn;
    public GameObject auxObject;

    public bool endProgress;
    public bool startProgress;
    public float timer;
    public float timerProgress;
    public float auxTimer;
    public Slider slider;

    void Reset(){
        endProgress = false;
        startProgress = false;
        timer = 0f;
        timerProgress = 0;
        auxTimer = 0f;
    }

    private void Update() {
        if(startProgress){
            timer += Time.deltaTime;
            slider.value = timer;
            if(timer >= timerProgress){
                Debug.Log("Acabou de preparar");
                startProgress = false;
                OnEndProgress();
            } 
        }
              
    }

    public void progress(){
        startProgress = true;
        foreach(FoodSO item in ingredients){
            auxTimer=item.timeProgress;
        }
        timerProgress+=auxTimer;
        slider.gameObject.SetActive(true);
        slider.maxValue = timerProgress;
    }
    
    public void AddIngredient(FoodSO ingredient){
        timer = 0;
        ingredients.Add(ingredient);
        if(auxObject==null)
            assetBenchType();
        if(!endProgress && benchType != BenchType.Storage)
            progress();
    }

    public FoodSO RemoveIngredient(FoodSO ingredient){
        FoodSO aux = ingredients.Find(x => x == ingredient);
        ingredients.Remove(ingredient);
        return aux;
    }

    public void OnEndProgress(){
        slider.gameObject.SetActive(false);
        endProgress = true;
    }

    public override void Pick(Player player)
    {
        if(endProgress){
            player.isHand = true;
            player.StatusAssetServerRpc(true);
            player.ChangeMeshHandServerRpc();
            if(ingredients.Count>0){
                player.ingredient = ingredients[0];
                ingredients.Clear();
            }
            //CanDestroyIngredientServerRpc();\
            //auxObject.GetComponent<Ingredient>().DestroySelf();
            DestroyImmediate(auxObject, true);
            Reset();
        }
        if(benchType == BenchType.Storage){
            player.isHand = true;
            player.StatusAssetServerRpc(true);
            player.ChangeMeshHandServerRpc();
            player.ingredient = RemoveIngredient(player.getIngredient);
            if(ingredients.Count == 0){
                DestroyImmediate(auxObject, true);
            }
        }

    }

    public override void Drop(Player player)
    {
        endProgress = false;
        player.StatusAssetServerRpc(false);
        AddIngredient(player.ingredient);
        player.isHand = false;
        player.ingredient = null;
    }

    void SpawnObject(int index){
        Vector3 spawnPosition = new Vector3(this.transform.position.x, 1.5f, this.transform.position.z);
        auxObject = Instantiate(assetBench[index], spawnPosition, Quaternion.identity);
    }

    void assetBenchType(){
        switch (benchType)
        {
            case BenchType.Oven:
                SpawnObject(0);
                break;
            case BenchType.Stove:
                SpawnObject(1);
                break;
            case BenchType.Board:
                SpawnObject(2);
                break;
            case BenchType.Storage:
                SpawnObject(3);
                break;
        }
    }
}
