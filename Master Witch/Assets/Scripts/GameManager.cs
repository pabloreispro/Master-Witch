using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Network;
using Unity.Netcode;
using System.Linq;
using UI;

public class GameManager : SingletonNetwork<GameManager>
{
    GameState gameState;
    [SerializeField] FoodDatabaseSO foodDatabase;
    [SerializeField] Bench[] benches;
    [SerializeField] RecipeSO[] recipeDatabase;
    public List<string> test;  

    Dictionary<int, float> ResultFinal = new Dictionary<int, float>();

    #region Properties
    public GameState GameState => gameState;
    public FoodDatabaseSO FoodDatabaseSO => foodDatabase;
    #endregion

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
    [ServerRpc(RequireOwnership = false)]
    public void OnClientsReadyServerRpc()
    {
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

    public void PlayerResultFinal(int playerID, float score){
        ResultFinal.Add(playerID, score);
    }

    public void PlayerElimation(){
        
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

    public void GetInitialRecipe()
    {
        RecipeSO initialRecipe = recipeDatabase.ElementAt(Random.Range(0, recipeDatabase.Length));
        Stack<string> steps = new Stack<string>();
        ExtractRecipeSteps(initialRecipe, steps);
        
        foreach (var step in steps)
        {
            test.Add(step);
        }
    }

    private void ExtractRecipeSteps(RecipeSO recipe, Stack<string> steps)
    {
        foreach (var condition in recipe.recipeConditions)
        {
            if (condition.type == RecipeCondition.ConditionType.BenchType)
            {
                steps.Push(condition.benchType.ToString());
            }
            else if (condition.type == RecipeCondition.ConditionType.Food)
            {
                foreach (var item in condition.foods)
                {
                    if (item is RecipeSO nestedRecipe)
                    {
                        ExtractRecipeSteps(nestedRecipe, steps);
                    }
                    else
                    {
                        steps.Push(item.name.ToString());
                    }
                }
            }
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