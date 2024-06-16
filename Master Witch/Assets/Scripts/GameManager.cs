using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Network;
using Unity.Netcode;
using System.Linq;
using UI;
using Unity.VisualScripting;
using UnityEngine.UI;
using JetBrains.Annotations;

public class GameManager : SingletonNetwork<GameManager>
{
    const int CHEFS_AMOUNT = 3;
    GameState gameState;
    [SerializeField] FoodDatabaseSO foodDatabase;
    [SerializeField] Bench[] benches;
    [SerializeField] RecipeSO[] recipeDatabase;
    [SerializeField] ChefSO[] chefsDatabase;
    public List<string> test;  
    public GameObject grid,horizontalGroupPrefab,imagePrefab;
    public Sprite plusSprite,equalsSprite,arrowSprite,benchOven,benchBoard,benchStove;
    Dictionary<int, float> ResultFinal = new Dictionary<int, float>();
    RecipeSO targetRecipe;
    List<ChefSO> chefs;
    #region Properties
    public GameState GameState => gameState;
    public FoodDatabaseSO FoodDatabaseSO => foodDatabase;
    public RecipeSO TargetRecipe => targetRecipe;
    public List<ChefSO> Chefs => chefs;
    #endregion

    public int numberPlayer;


    void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        int i = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count % SceneManager.Instance.spawnPlayersMarket.Count;

        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Position = SceneManager.Instance.spawnPlayersMarket.ElementAt(i).position;
        //response.Rotation = Quaternion.Euler(0f,180f,0f);
    }

    void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
        
    }

    
    
    public async void HostRelay()
    {
        NetworkManagerUI.Instance.EnableHUD(false);
        await LobbyManager.Instance.StartHostRelay();
    }

    public void OnClientsReady()
    {
        Debug.Log("Chamou OnClientReady");
        SceneManager.Instance.ChangeSceneServerRpc(true, false);
        SceneManager.Instance.StartMarket();
        NetworkManagerUI.Instance.OnGameStartedClientRpc();
        for (int i = 0; i < benches.Length; i++)
        {
            var player = PlayerNetworkManager.Instance.GetPlayerByIndex(i);
            if (player != null)
                benches[i].SetPlayer(player);
            else break;
        }
    }

    public void JoinRelay(string joinCode) => StartClientRelay(joinCode);
    async void StartClientRelay(string joinCode)
    {
        if (IsHost) return;
        NetworkManagerUI.Instance.EnableHUD(false);
        Debug.Log($"start relay {joinCode}");
        await LobbyManager.Instance.StartClientWithRelay(joinCode);
        Debug.Log($"join");
    }

    public void ChangeGameState(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.None:
                break;
            case GameState.Waiting:
                break;
            case GameState.Starting:
                break;
            case GameState.Playing:
                //StartGame();
                break;
            case GameState.Ending:
                break;
            default:
                break;
        }
    }

    public RecipeSO GetValidRecipe(List<FoodSO> ingredients, BenchType benchType)
    {
        List<RecipeSO> validRecipes = new List<RecipeSO>();
        Debug.Log(foodDatabase.RecipeContainer.Count);
        foreach (var recipe in foodDatabase.RecipeContainer)
        {
            Debug.Log(recipe.name);
            if(recipe.CheckConditions(ingredients, benchType))
                validRecipes.Add(recipe);
        }
        if (validRecipes.Count > 0)
            return validRecipes[Random.Range(0, validRecipes.Count)];
        else
        {
            Debug.Log($"No valid recipes, returning default");
            return foodDatabase.DefaultRecipe;
        }
    }
    public void InitializeGame()
    {
        GetInitialRecipe();
        SelectChefs();
    }
    void GetInitialRecipe()
    {
        targetRecipe = recipeDatabase.ElementAt(Random.Range(0, recipeDatabase.Length));
        
        foreach (var step in ExtractRecipeSteps(targetRecipe))
        {
            step.transform.SetParent(grid.transform);
        }
    }
    void SelectChefs()
    {
        chefs = new List<ChefSO>();
        for (int i = 0; i < CHEFS_AMOUNT; i++)
        {
            ChefSO chef = null;
            while (chef == null)
            {
                chef = chefsDatabase[Random.Range(0, chefsDatabase.Length)];
                if (chefs.Contains(chef))
                    chef = null;
            }
            chefs.Add(chef);
        }
    }
    private IEnumerable<GameObject> ExtractRecipeSteps(RecipeSO recipe)
    {
        var ingredients = recipe.recipeConditions.FirstOrDefault(r => r.type == RecipeCondition.ConditionType.Food);
        var bench = recipe.recipeConditions.FirstOrDefault(r => r.type == RecipeCondition.ConditionType.BenchType);
        foreach (var condition in recipe.recipeConditions)
        {
            if (condition.type == RecipeCondition.ConditionType.Food)
            {
                foreach (var item in condition.foods)
                {
                    if (item is RecipeSO nestedRecipe)
                        foreach(var s in ExtractRecipeSteps(nestedRecipe))
                            yield return s;
                            
                }
            }
        }
        var step = Instantiate(horizontalGroupPrefab);
        
        if (ingredients != null && ingredients.foods != null)
        {
            for (int i = 0; i < ingredients.foods.Count; i++)
            {
                var ingredient = ingredients.foods[i];
                var item = Instantiate(imagePrefab, step.transform);
                var image = item.GetComponent<Image>();
                image.sprite = ingredient.imageFood; 

                
                if (i < ingredients.foods.Count - 1)
                {
                    var plus = Instantiate(imagePrefab, step.transform);
                    var plusImage = plus.GetComponent<Image>();
                    plusImage.sprite = plusSprite;
                }
            }
        }

        
        if (bench != null)
        {
            var arrow = Instantiate(imagePrefab, step.transform);
            var arrowImage = arrow.GetComponent<Image>();
            arrowImage.sprite = arrowSprite; 

            var benchItem = Instantiate(imagePrefab, step.transform);
            var benchImage = benchItem.GetComponent<Image>();

            if(bench.benchType == BenchType.Oven)
            {
                benchImage.sprite = benchOven; 
            }
            else if (bench.benchType == BenchType.Stove)
            {
                benchImage.sprite = benchStove;
            }
            else
            {
                benchImage.sprite = benchBoard;
            }
            
        }

        
        var equals = Instantiate(imagePrefab, step.transform);
        var equalsImage = equals.GetComponent<Image>();
        equalsImage.sprite = equalsSprite;

        
        var result = Instantiate(imagePrefab, step.transform);
        var resultImage = result.GetComponent<Image>();
        resultImage.sprite = recipe.imageFood; 

        yield return step;
        //yield return horizontalGroupPrefab; //string.Join(" + ", ingredients.foods.Select(f => f.name)) + " -> " + bench.benchType + " = " + recipe.name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyPlayersServerRpc(int playerID, bool isOn){
        AttToggleClientRpc(playerID, isOn);
        
    }

    [ClientRpc]
    public void AttToggleClientRpc(int playerID, bool isOn){
        NetworkManagerUI.Instance.UpdateToggle(playerID, isOn);
    }

    [ClientRpc]
    public void OnReturnMarketClientRpc(){
        NetworkManagerUI.Instance.finalPanel.SetActive(false);
        //Reset();
    }

    public void OnReturnMarket(){
        OnReturnMarketClientRpc();
    }


    public void Reset(){
        foreach(Interactable objectScene in FindObjectsOfType<Interactable>()){
            if((objectScene as Tool) != null &&(objectScene as Tool).isHandTool)
                objectScene.DestroySelf();
            if((objectScene as Ingredient) != null &&(objectScene as Ingredient).isHandIngredient)
                objectScene.DestroySelf();
        }
        for(int i=0; i<numberPlayer; i++){
            NetworkManagerUI.Instance.playerFinalCheck[i].isOn = false;
        }
    }
}
public enum GameState
{
    None,
    Waiting,
    Starting,
    Playing,
    Ending
}