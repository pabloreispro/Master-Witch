using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;
using Network;
using Mono.Cecil;

public class GameManager : Singleton<GameManager>
{
    GameState gameState;
    [SerializeField] FoodDatabaseSO foodDatabase;
    [SerializeField] Bench[] benches;

    #region Properties
    public GameState GameState => gameState;
    public FoodDatabaseSO FoodDatabaseSO => foodDatabase;
    #endregion


    public void StartGame()
    {
        for (int i = 0; i < benches.Length; i++)
        {
            var player = PlayerNetworkManager.Instance.GetPlayerByIndex(i);
            if (player != null)
                benches[i].SetPlayer(player);
            else break;
        }
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