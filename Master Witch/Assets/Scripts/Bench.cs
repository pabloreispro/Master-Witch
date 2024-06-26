using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SO;
using Network;
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

    [Header("Bench config")]
    public Tool toolInBench;
    public RecipeSO targetRecipe;
    public BenchType benchType;
    public GameObject auxObject;
    public Transform positionBasket;
    public int multiBenchSpecial;
    public bool SpecialBench;

    [Header("Progress Recipe")]
    public bool endProgress;
    public bool startProgress;
    public float timer;
    public float timerProgress;
    public float auxTimer;

    [Header("UI")]
    public Slider slider;
    public GameObject inventory;
    public StorageController storage;

    


    private void Start()
    {
        storage = GetComponent<StorageController>();
        if(!SpecialBench){
            multiBenchSpecial = 1;
        }
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
            timer += Time.deltaTime * multiBenchSpecial;
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
        targetRecipe = GameManager.Instance.GetValidRecipe(toolInBench.foodList, benchType);
        startProgress = true;
        foreach (FoodSO item in toolInBench.foodList)
        {
            auxTimer = item.timeProgress;
        }
        timerProgress += auxTimer;
        slider.gameObject.SetActive(true);
        slider.maxValue = timerProgress;
    }

    public void AddIngredient(FoodSO ingredient) => AddIngredient(new RecipeData(ingredient));
    public void AddIngredient(RecipeData recipeData)
    {
        timer = 0;
        toolInBench.ingredients.Add(recipeData);
        Debug.Log($"{recipeData.TargetFood} {recipeData.UtilizedIngredients.Count}");
        if (!endProgress && benchType != BenchType.General)
            progress();
    }


    public FoodSO RemoveIngredient(FoodSO ingredient)
    {
        FoodSO aux = null;
        for (int i = 0; i < toolInBench.ingredients.Count; i++)
        {
            if (toolInBench.ingredients[i].TargetFood == ingredient)
            {
                aux = toolInBench.ingredients[i].TargetFood;
                RemoveIngredientServerRpc(i);
            }
        }
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
        if (endProgress)
        {
            if(targetRecipe.finishRecipe){
                player.GetComponentInChildren<Tool>().ingredients.Add(new RecipeData(targetRecipe, toolInBench.ingredients));
                toolInBench.DestroySelf();
            }else{
                var recipeData = new RecipeData(targetRecipe, toolInBench.ingredients);
                toolInBench.ingredients.Clear();
                toolInBench.ingredients.Add(recipeData);
                toolInBench.transform.position = player.assetIngredient.transform.position;
                toolInBench.GetComponent<NetworkObject>().TrySetParent(player.transform);
            }
            Reset();
        }
        if(benchType == BenchType.General){
            player.isHand.Value = true;
            player.ChangeState(PlayerState.Interact);
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
            player.isHand.Value = false;
            player.isHandBasket.Value = false;
        }
        else if(benchType == BenchType.General){
            if((interact as Tool) != null){
                if(toolInBench == null){
                    PositionBench(interact);
                    player.isHand.Value = false;
                }else{
                    toolInBench.ingredients.AddRange((interact as Tool).ingredients);
                    interact.DestroySelf();
                    player.isHand.Value = false;
                }
            }
            if(toolInBench != null && (interact as Ingredient)!=null){
                AddIngredient((interact as Ingredient).food);
                player.GetComponentInChildren<Ingredient>().DestroySelf();
                player.isHand.Value = false;
            }
            else if((interact as Ingredient)!=null){
                interact.gameObject.transform.position = auxObject.transform.position;
                interact.GetComponent<NetworkObject>().TrySetParent(this.transform);
            }
        }
        else if(benchType != BenchType.Storage)
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
                    player.isHand.Value = false;
                }
            }
            if((interact as Ingredient) != null){
                endProgress = false;
                AddIngredient((interact as Ingredient).food);
                interact.DestroySelf();
                player.isHand.Value = false;
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
    
    public void StorageInitialize(){
        StoreServerRpc();
        inventory.SetActive(true);
    }
    public void StorageDisable(){
        inventory.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StoreServerRpc(){
        StoreClientRpc();
    }
    [ClientRpc]
    public void StoreClientRpc(){
        if(toolInBench!=null){
            storage.Initialize(toolInBench.foodList);
        }
    }
}
