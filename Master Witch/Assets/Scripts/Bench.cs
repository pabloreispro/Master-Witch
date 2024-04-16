using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SO;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum BenchType {Oven, Stove, Board, Storage, Basket, TrashBin, General}
public class Bench : Interactable
{
    int playerID;
    Player targetPlayer;
    public ToolsSO tool;

    [SerializeField]
    List<FoodSO> ingredients = new List<FoodSO>();
    RecipeSO targetRecipe;
    public BenchType benchType;

    public bool isSpawn;
    public GameObject auxObject;

    public bool endProgress;
    public bool startProgress;
    public float timer;
    public float timerProgress;
    public float auxTimer;
    public Slider slider;

    void Reset()
    {
        endProgress = false;
        startProgress = false;
        timer = 0f;
        timerProgress = 0;
        auxTimer = 0f;
        tool = null;
    }

    private void Update()
    {
        if (startProgress)
        {
            timer += Time.deltaTime;
            slider.value = timer;
            if (timer >= timerProgress)
            {
                Debug.Log("Acabou de preparar");
                startProgress = false;
                OnEndProgress();
            }
        }
    }
    public void progress()
    {
        startProgress = true;
        foreach (FoodSO item in ingredients)
        {
            auxTimer = item.timeProgress;
        }
        timerProgress += auxTimer;
        slider.gameObject.SetActive(true);
        slider.maxValue = timerProgress;
    }

    public void AddIngredient(FoodSO ingredient)
    {
        timer = 0;
        ingredients.Add(ingredient);
        targetRecipe = GameManager.Instance.GetValidRecipe(ingredients);
        if (!endProgress && benchType != BenchType.Storage)
            progress();
    }

    public FoodSO RemoveIngredient(FoodSO ingredient)
    {
        FoodSO aux = ingredients.Find(x => x == ingredient);
        ingredients.Remove(ingredient);
        return aux;
    }

    public void OnEndProgress()
    {
        //ingredients.Clear();
        //ingredients.Add(targetRecipe);
        slider.gameObject.SetActive(false);
        endProgress = true;
    }
    public void SetPlayer(Player player)
    {
        targetPlayer = player;
    }

    public override void Pick(Player player)
    {
        //if (player != targetPlayer) return;
        if (endProgress)
        {
            player.isHand = true;
            player.tool = tool;
            player.StatusAssetServerRpc(true);
            player.ChangeMeshHandToolServerRpc();
            if (ingredients.Count > 0)
            {
                //player.ingredient = ingredients[0];
                player.recipeIngredients.Clear();
                player.ingredient = targetRecipe;
                foreach(FoodSO item in ingredients){
                    player.recipeIngredients.Add(item);
                }
                ingredients.Clear();
            }
            //CanDestroyIngredientServerRpc();\
            //auxObject.GetComponent<Ingredient>().DestroySelf();
            DestroyImmediate(auxObject, true);
            Reset();
        }
        if (benchType == BenchType.Storage)
        {
            player.isHand = true;
            player.StatusAssetServerRpc(true);
            player.ChangeMeshHandServerRpc();
            player.ingredient = RemoveIngredient(player.getIngredient);
            if (ingredients.Count == 0)
            {
                DestroyImmediate(auxObject, true);
            }
        }

    }

    public override void Drop(Player player)
    {
        //if (player != targetPlayer) return;
        if(benchType == BenchType.TrashBin)
        {
            player.StatusAssetServerRpc(false);
            player.isHand = false;
            player.tool = null;
            player.ingredient = null;
            player.ingredientsBasket.Clear();
            player.recipeIngredients.Clear();
        }
        else
        {
            if (player.tool != null && tool == null)
            {
                if (player.tool.benchType == benchType)
                {
                    tool = player.tool;
                    assetBenchType(tool.prefab);
                    player.tool = null;
                    player.isHand = false;
                    player.StatusAssetServerRpc(false);
                    
                }
            }
            if (player.ingredient != null && tool != null)
            {
                endProgress = false;
                AddIngredient(player.ingredient);
                player.isHand = false;
                player.ingredient = null;
                player.StatusAssetServerRpc(false);
            }
        }
        

    }

    void SpawnObject(GameObject assetBench)
    {
        Vector3 spawnPosition = new Vector3(this.transform.position.x, 1.5f, this.transform.position.z);
        auxObject = Instantiate(assetBench, spawnPosition, Quaternion.identity);
    }

    void assetBenchType(GameObject gameObject)
    {
        switch (benchType)
        {
            case BenchType.Oven:
                SpawnObject(gameObject);
                break;
            case BenchType.Stove:
                SpawnObject(gameObject);
                break;
            case BenchType.Board:
                SpawnObject(gameObject);
                break;
            case BenchType.Storage:
                SpawnObject(gameObject);
                break;
        }
    }
}
