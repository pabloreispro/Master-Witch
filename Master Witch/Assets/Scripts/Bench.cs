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
using UnityEngine.VFX;

public enum BenchType {Oven, Stove, Board, Storage, Basket, TrashBin, General}
public class Bench : Interactable
{
    int playerID;
    Player targetPlayer;

    [Header("Bench config")]
    //public Tool toolInBench;
    public RecipeSO targetRecipe;
    public BenchType benchType;
    private GameObject auxObject;
    private int multiBenchSpecial;
    private bool SpecialBench;
    public List<RecipeData> ingredients = new List<RecipeData>();

    [Header("Progress Recipe")]
    public bool endProgress;
    private bool startProgress;
    private float timer;
    private float timerProgress;
    private float auxTimer;
    public NetworkVariable<bool> isPreparingBusen = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isPreparingAlmof = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isPreparingBoard = new NetworkVariable<bool>(false);
    public VisualEffect smokeBusenVFX,smokeAlmofVFX,smokeBoardVFX,fireBusenVFX;
    public ParticleSystem fire;
    [Header("UI")]
    public Slider slider;
    //public GameObject inventory;
    public StorageController storage;

    
    public List<FoodSO> foodList 
    {
        get
        {
            var list = new List<FoodSO>();
            foreach (var item in ingredients)
            {
                list.Add(item.TargetFood);
            }
            return list;
        }
    }

    private void Start()
    {
        if(isPreparingBusen != null)
        {
            isPreparingBusen.OnValueChanged += (a,b)=>smokeBusenVFX.SetBool("isPreparing",isPreparingBusen.Value);
            isPreparingBusen.OnValueChanged += (a,b)=>fireBusenVFX.SetBool("isPreparing",isPreparingBusen.Value);
        }
        if(isPreparingAlmof != null){isPreparingAlmof.OnValueChanged += (a,b)=>smokeAlmofVFX.SetBool("isPreparing",isPreparingAlmof.Value);}
        if(isPreparingBoard != null){isPreparingBoard.OnValueChanged += (a,b)=>smokeBoardVFX.SetBool("isPreparing",isPreparingBoard.Value);}

        storage = GetComponent<StorageController>();
        if(!SpecialBench){
            multiBenchSpecial = 1;
        }
    }

    public void Reset()
    {
        endProgress = false;
        startProgress = false;
        timer = 0f;
        timerProgress = 0;
        auxTimer = 0f;
    }

    /*private void Update()
    {
        if(benchType == BenchType.Storage){
            toolInBench = this.GetComponentInChildren<Tool>();
            if(toolInBench != null)
                toolInBench.transform.position = positionBasket.position;
        }
        if (startProgress)
        {
            if(benchType == BenchType.Stove)
            {
                timer += Time.deltaTime * multiBenchSpecial;
                slider.value = timer;
                isPreparingBusen.Value = true;
                if (timer >= timerProgress)
                {
                    startProgress = false;
                    isPreparingBusen.Value = false;
                    OnEndProgress();
                }
            }
            else if(benchType == BenchType.Oven)
            {
                timer += Time.deltaTime * multiBenchSpecial;
                slider.value = timer;
                isPreparingAlmof.Value = true;
                if (timer >= timerProgress)
                {
                    startProgress = false;
                    isPreparingAlmof.Value = false;
                    OnEndProgress();
                }
            }
            else if(benchType == BenchType.Board)
            {
                timer += Time.deltaTime * multiBenchSpecial;
                slider.value = timer;
                isPreparingBoard.Value = true;
                if (timer >= timerProgress)
                {
                    startProgress = false;
                    isPreparingBoard.Value = false;
                    OnEndProgress();
                }
            }
            else
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
    }*/
    public void progress()
    {
        targetRecipe = GameManager.Instance.GetValidRecipe(foodList, benchType);
        startProgress = true;
        foreach (FoodSO item in foodList)
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
        ingredients.Add(recipeData);
        Debug.Log($"{recipeData.TargetFood} {recipeData.UtilizedIngredients.Count}");
        //if (!endProgress && benchType != BenchType.General)
        //progress();
    }


    public FoodSO RemoveIngredient(FoodSO ingredient)
    {
        FoodSO aux = null;
        for (int i = 0; i < ingredients.Count; i++)
        {
            if (ingredients[i].TargetFood == ingredient)
            {
                aux = ingredients[i].TargetFood;
                RemoveIngredientServerRpc(i);
            }
        }
        return aux;
    }
    [ServerRpc(RequireOwnership = false)]
    void RemoveIngredientServerRpc(int recipeSlot)
    {
        Debug.Log(recipeSlot);
        ingredients.RemoveAt(recipeSlot);
        RemoveIngredientClientRpc(recipeSlot);
    }
    [ClientRpc]
    void RemoveIngredientClientRpc(int recipeSlot)
    {
        if (IsServer) return;
        ingredients.RemoveAt(recipeSlot);
    }
    public void OnEndProgress()
    {
        //ingredients.Clear();
        //ingredients.Add(targetRecipe);
        
        endProgress = true;
    }
    public void SetPlayer(Player player)
    {
        targetPlayer = player;
    }

    /*public override void Pick(Player player)
    {
        //if (player != targetPlayer) return;
        player.isHand.Value = true;
        player.ChangeState(PlayerState.Interact);
        if (endProgress)
        {
            slider.gameObject.SetActive(false);
            if(targetRecipe.finishRecipe){
                player.GetComponentInChildren<Tool>().ingredients.Add(new RecipeData(targetRecipe, toolInBench.ingredients));
                toolInBench.DestroySelf();
                Debug.Log("1");
            }else{
                var recipeData = new RecipeData(targetRecipe, toolInBench.ingredients);
                toolInBench.ingredients.Clear();
                toolInBench.ingredients.Add(recipeData);
                toolInBench.transform.position = player.boneItem.transform.position;
                toolInBench.gameObject.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
                toolInBench.GetComponent<NetworkObject>().TrySetParent(player.transform);
                player.SetItemHandClientRpc(toolInBench.gameObject);
                Debug.Log("2");
            }
            Reset();
        }
        if(benchType == BenchType.General){
            if(this.GetComponentInChildren<Ingredient>()!=null)
            {
                this.GetComponentInChildren<Ingredient>().gameObject.transform.position = player.boneItem.transform.position;
                this.GetComponentInChildren<Ingredient>().gameObject.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
                this.GetComponentInChildren<Ingredient>().GetComponent<NetworkObject>().TrySetParent(player.transform);               
                player.SetItemHandClientRpc(GetComponentInChildren<Ingredient>().gameObject);
            }
            if(toolInBench!=null)
            {
                toolInBench.transform.position = player.boneItem.transform.position;
                toolInBench.GetComponentInChildren<NetworkObject>().TrySetParent(player.transform);
                toolInBench.GetComponent<FollowTransform>().targetTransform = player.boneItem.transform;
                player.SetItemHandClientRpc(toolInBench.gameObject);
                toolInBench=null;
            }
            Reset();
        }

    }*/
    public void GetPlayer(Player player)
    {
        
    }
    /*public override void Drop(Player player)
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
                PositionBench(interact);
                player.isHand.Value = false;
            }
        }
        if(benchType != BenchType.Storage)
        {
            if((interact as Tool) != null)
            {
                if ((interact as Tool).tool.benchType == benchType)
                {
                    interact.GetComponent<FollowTransform>().targetTransform = null;
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
    }*/

    public void PositionBench(Interactable interact){
        interact.GetComponent<FollowTransform>().targetTransform = null;
        interact.gameObject.transform.rotation = Quaternion.identity;
        interact.gameObject.transform.position = auxObject.transform.position;
        /*if(interact as Tool){
            toolInBench = interact.GetComponent<Tool>();
        }*/
        interact.GetComponent<NetworkObject>().TrySetParent(this.transform);
    }
    
    /*public void StorageInitialize(){
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
    }*/
}
