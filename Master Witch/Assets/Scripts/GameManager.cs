using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;

public class GameManager : Singleton<GameManager>
{
    GameState gameState;
    [SerializeField] FoodDatabaseSO foodDatabase;

    #region Properties
    public GameState GameState => gameState;
    public FoodDatabaseSO FoodDatabaseSO => foodDatabase;
    #endregion


}
public enum GameState
{
    None,
    Waiting,
    Starting,
    Playing,
    Ending
}