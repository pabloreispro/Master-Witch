using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SO;
using Game.UI;
using Network;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public enum BenchType {BusenBurner, Cutting, Mortar, Alembic, Cauldron, Furnace, MysteriousFountain, Refrigeration, Well, Book, Table, Market, Storage, Tool, Trash, Free}
public class Bench : Interactable
{
    int playerID;
    public PlayerMovement _player;
    [Header("Bench config")]
    //public Tool toolInBench;
    public RecipeSO targetRecipe;
    public BenchType benchType;
    public Transform _auxObject;
    public GameObject objectInBench;
    public List<RecipeData> ingredients = new List<RecipeData>();
    public float timerMultiply;
    public bool isPerformed;
    public bool workBench;

    [Header("Progress Recipe")]
    public bool endProgress;
    private float _timer;
    private float _timerProgress;
    private float _auxTimer;
    public NetworkVariable<bool> isPreparing = new NetworkVariable<bool>(false);
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
        _timer = 0f;
        isPreparing.Value = false;
        _timerProgress = 0;
        _auxTimer = 0f;
        ingredients.Clear();
        targetRecipe = null;
        objectInBench = null;
    }

    private void Update()
    {
        if (isPreparing.Value && workBench)
        {
            _timer += Time.deltaTime * timerMultiply;
            slider.value = _timer;
            if (_timer >= _timerProgress)
            {
                ChangeVariableServerRpc(false);
                OnEndProgressServerRpc();
            }
        }
    }
    public void progress()
    {
        targetRecipe = GameManager.Instance.GetValidRecipe(foodList, benchType);
        _auxTimer = targetRecipe.timeProgress;
        _timerProgress += _auxTimer;
        slider.gameObject.SetActive(true);
        slider.maxValue = _timerProgress;
    }

    public void AddIngredient(Ingredient ingredient)
    {
        if (ingredient.itemsUsed.Count <= 0 || ingredient.itemsUsed == null) 
            AddIngredient(new RecipeData(ingredient.food));
        else
            AddIngredient(new RecipeData(ingredient.food, ingredient.itemsUsed));
    }
    public void AddIngredient(FoodSO ingredient) => AddIngredient(new RecipeData(ingredient));
    public void AddIngredient(RecipeData recipeData)
    {
        _timer = 0;
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
    [ServerRpc(RequireOwnership = false)]
    public void OnEndProgressServerRpc()
    {
        isPreparing.Value = false;
        if(endProgress==false && workBench == true){
            if(GetComponentInChildren<Ingredient>()!=null)
                GetComponentInChildren<Ingredient>().DestroySelf();
            var recipeData = new RecipeData(targetRecipe, ingredients);
            var objectSpawn = Instantiate(recipeData.TargetFood.foodPrefab, new Vector3(_auxObject.position.x, _auxObject.position.y, _auxObject.position.z), Quaternion.identity);
            objectSpawn.GetComponent<NetworkObject>().Spawn();
            objectSpawn.GetComponentInChildren<NetworkObject>().TrySetParent(this.transform);
            objectSpawn.gameObject.GetComponent<Rigidbody>().useGravity = false;
            objectSpawn.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            objectInBench = objectSpawn;
            var ingredient = objectSpawn.GetComponent<Ingredient>();
            ingredient.food = targetRecipe;
            ingredient.itemsUsed.Add(recipeData);
        }
        OnEndProgressClientRpc();
    }
    [ClientRpc]
    public void OnEndProgressClientRpc(){
        slider.gameObject.SetActive(false);
        endProgress = true;
    }

    public void PositionBench(Interactable interact){
        interact.GetComponent<FollowTransform>().targetTransform = null;
        interact.GetComponent<NetworkObject>().TrySetParent(this.transform);
        interact.gameObject.transform.rotation = Quaternion.identity;
        interact.gameObject.transform.position = _auxObject.position;
        interact.gameObject.GetComponent<Rigidbody>().useGravity = false;
        interact.gameObject.GetComponent<Rigidbody>().isKinematic = true;

    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeVariableServerRpc(bool v){
        isPreparing.Value = v;
    }
    
    
}
