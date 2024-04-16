using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Network;
using Unity.Netcode;
using System.Linq;

public class GameManager : Singleton<GameManager>
{
    GameState gameState;
    [SerializeField] FoodDatabaseSO foodDatabase;
    [SerializeField] Bench[] benches;
    public SceneManager scene;

    #region Properties
    public GameState GameState => gameState;
    public FoodDatabaseSO FoodDatabaseSO => foodDatabase;
    #endregion

    void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            int i = PlayerNetworkManager.Instance.playerList.Values.ToList().Count % SceneManager.Instance.spawnPlayersMarket.Count;
             
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = SceneManager.Instance.spawnPlayersMarket.ElementAt(i).position;
            //response.Rotation = Quaternion.Euler(0f,180f,0f);
        }

    void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
    }
    public void StartGame()
    {
        scene.ChangeSceneServerRpc(true, false);
        for (int i = 0; i < benches.Length; i++)
        {
            var player = PlayerNetworkManager.Instance.GetPlayerByIndex(i);
            if (player != null)
                benches[i].SetPlayer(player);
            else break;
        }
        var g = FindAnyObjectByType<SceneManager>();
        g.StartMarket();
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

    public RecipeSO GetValidRecipe(List<FoodSO> ingredients)
    {
        List<RecipeSO> validRecipes = new List<RecipeSO>();
        Debug.Log(foodDatabase.RecipeContainer.Count);
        foreach (var recipe in foodDatabase.RecipeContainer)
        {
            Debug.Log(recipe.name);
            if(recipe.CheckConditions(ingredients))
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
}
public enum GameState
{
    None,
    Waiting,
    Starting,
    Playing,
    Ending
}