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
    public List<RecipeData> ingredients = new List<RecipeData>();

    [Header("Progress Recipe")]
    public bool endProgress;
    public bool startProgress;
    public float timer;
    private float timerProgress;
    private float auxTimer;
    public NetworkVariable<bool> isPreparing = new NetworkVariable<bool>(false);
    public VisualEffect[] visualEffect;
    public ParticleSystem particleSystemBench;
    
    [Header("UI")]
    public Slider slider;
    

    
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

    public void Reset()
    {
        endProgress = false;
        startProgress = false;
        timer = 0f;
        timerProgress = 0;
        auxTimer = 0f;
    }

    private void Update()
    {
        if (startProgress)
        {
            timer += Time.deltaTime;
            slider.value = timer;
            isPreparing.Value = true;
            if (timer >= timerProgress)
            {
                startProgress = false;
                isPreparing.Value = false;
                OnEndProgress();
            }
        }
    }
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

    public void PositionBench(Interactable interact){
        interact.GetComponent<FollowTransform>().targetTransform = null;
        interact.gameObject.transform.rotation = Quaternion.identity;
        interact.gameObject.transform.position = auxObject.transform.position;
        interact.GetComponent<NetworkObject>().TrySetParent(this.transform);
    }
    
    
}
