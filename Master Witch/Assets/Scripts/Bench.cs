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
    public Player auxPlayer;
    public Tool toolInBench;
    RecipeSO targetRecipe;
    public BenchType benchType;
    public GameObject auxObject;
    public bool endProgress;
    public bool startProgress;
    public float timer;
    public float timerProgress;
    public float auxTimer;
    public Slider slider;
    public GameObject inventory;
    StorageController storage;
    
    public Transform positionBasket;


    private void Start()
    {
        storage = GetComponent<StorageController>();

    }

    void Reset()
    {
        endProgress = false;
        startProgress = false;
        timer = 0f;
        timerProgress = 0;
        auxTimer = 0f;
    }

    private void Update()
    {
        if(benchType == BenchType.Storage){
            toolInBench = this.GetComponentInChildren<Tool>();
            if(toolInBench != null)
                toolInBench.transform.position = positionBasket.position;
        }
        if (startProgress)
        {
            timer += Time.deltaTime;
            slider.value = timer;
            if (timer >= timerProgress)
            {
                startProgress = false;
                OnEndProgress();
            }
        }
    }
    public void progress()
    {
        targetRecipe = GameManager.Instance.GetValidRecipe(toolInBench.ingredients, benchType);
        startProgress = true;
        foreach (FoodSO item in toolInBench.ingredients)
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
        toolInBench.ingredients.Add(ingredient);
        if (!endProgress && benchType != BenchType.General)
            progress();
    }


    public FoodSO RemoveIngredient(FoodSO ingredient)
    {
        FoodSO aux = toolInBench.ingredients.Find(x => x == ingredient);
        RemoveIngredientServerRpc(toolInBench.ingredients.IndexOf(ingredient));
        return aux;
    }
    [ServerRpc(RequireOwnership = false)]
    void RemoveIngredientServerRpc(int recipeSlot)
    {
        Debug.Log(recipeSlot);
        toolInBench.ingredients.RemoveAt(recipeSlot);
        RemoveIngredientClientRpc(recipeSlot);
    }
    [ClientRpc]
    void RemoveIngredientClientRpc(int recipeSlot)
    {
        if (IsServer) return;
        toolInBench.ingredients.RemoveAt(recipeSlot);
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
        auxPlayer = player;
        if (endProgress)
        {
            player.isHand = true;
            if(targetRecipe.finishRecipe){
                var objectSpawn = Instantiate(targetRecipe.foodPrefab, new Vector3(player.assetIngredient.transform.position.x, 1.0f, player.assetIngredient.transform.position.z), Quaternion.identity);
                objectSpawn.GetComponent<NetworkObject>().Spawn();
                objectSpawn.GetComponent<NetworkObject>().TrySetParent(player.transform);
                objectSpawn.GetComponent<Tool>().ingredients.AddRange(toolInBench.ingredients);
                toolInBench.DestroySelf();
            }else{
                toolInBench.ingredients.Clear();
                toolInBench.ingredients.Add(targetRecipe);
                toolInBench.transform.position = player.assetIngredient.transform.position;
                toolInBench.GetComponent<NetworkObject>().TrySetParent(player.transform);
            }
            Reset();
        }
        if (benchType == BenchType.Storage)
        {
            if (player.IsOwner && !storage.Active)
            {
                storage.Initialize(toolInBench.ingredients);
                inventory.SetActive(true);
            }
        }
        if(benchType == BenchType.General){
            player.isHand = true;
            if(this.GetComponentInChildren<Ingredient>()!=null)
                this.GetComponentInChildren<Ingredient>().GetComponent<NetworkObject>().TrySetParent(player.transform);
            if(toolInBench!=null)
            {
                toolInBench.GetComponentInChildren<NetworkObject>().TrySetParent(player.transform);
                toolInBench=null;
            }
            Reset();
        }

    }
    public void GetPlayer(Player player)
    {
        
    }
    public override void Drop(Player player)
    {
        var interact = player.GetComponentInChildren<Interactable>();
        //if (player != targetPlayer) return;
        if(benchType == BenchType.TrashBin)
        {
            interact.DestroySelf();  
            player.isHandBasket = false;
        }
        else if(benchType == BenchType.General){
            if((interact as Tool) != null){
                if(toolInBench == null){
                    PositionBench(interact);
                }else{
                    toolInBench.ingredients.AddRange((interact as Tool).ingredients);
                    interact.DestroySelf();
                }
            }
            if(toolInBench != null && (interact as Ingredient)!=null){
                AddIngredient((interact as Ingredient).food);
                player.GetComponentInChildren<Ingredient>().DestroySelf();
            }
            else if((interact as Ingredient)!=null){
                interact.gameObject.transform.position = auxObject.transform.position;
                interact.GetComponent<NetworkObject>().TrySetParent(this.transform);
            }
        }
        else
        {
            if((interact as Tool) != null)
            {
                if ((interact as Tool).tool.benchType == benchType)
                {
                    PositionBench(interact);
                    if(toolInBench.ingredients.Count > 0){
                        endProgress = false;
                        progress();
                    }
                    player.isHand = false;
                }
            }
            if((interact as Ingredient) != null){
                endProgress = false;
                AddIngredient((interact as Ingredient).food);
                interact.DestroySelf();
                player.isHand = false;
            }
        }
    }

    public void PositionBench(Interactable interact){
        interact.gameObject.transform.position = auxObject.transform.position;
        if(interact as Tool){
            toolInBench = interact.GetComponent<Tool>();
        }
        interact.GetComponent<NetworkObject>().TrySetParent(this.transform);
    }
}
