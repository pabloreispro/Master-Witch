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
using Game.UI;
using Game.SceneGame;

public class GameManager : SingletonNetwork<GameManager>
{
    const int CHEFS_AMOUNT = 3;
    const int TOTAL_ROUNDS = 2;
    GameState gameState;
    [SerializeField] FoodDatabaseSO foodDatabase;
    [SerializeField] Bench[] benches;
    [SerializeField] MeshRenderer[] benchColorRenderer;
    [SerializeField] RecipeSO[] recipeDatabase;
    [SerializeField] ChefSO[] chefsDatabase;
    [SerializeField] Transform[] chefsSpawn;
  
    public Text RecipeText;
    
    RecipeSO targetRecipe;
    public float matchStartTime;
    public List<ChefSO> chefs;
    public List<GameObject> chefsGO;
    List<GameObject> recipeSteps = new List<GameObject>();
    #region Properties
    public GameState GameState => gameState;
    public FoodDatabaseSO FoodDatabaseSO => foodDatabase;
    public RecipeSO TargetRecipe => targetRecipe;
    public List<ChefSO> Chefs => chefs;
    public int TotalRounds => TOTAL_ROUNDS;
    public int CurrentRound { get; private set; }
    #endregion

    //public int numberPlayer;


    void Start()
    {
        if (NetworkManager.IsServer)
            StartGame();
    }

    public void StartGame()
    {
        Debug.Log("Game Started");
        CurrentRound = 1;
        OnGameStartedClientRpc();
    }
    [ClientRpc]
    void OnGameStartedClientRpc()
    {
        NewCamController.Instance.IntroClient();
        GameInterfaceManager.Instance.OnGameStartedClient();
        ResetInfo();
    }
    void ResetInfo()
    {
        LobbyManager.Instance.ResetJoinedLobby();
        //EliminationPlayer.Instance.Reset();
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

    public void InitializeGame(bool updateChefs = true)
    {
        int recipeIndex = Random.Range(0, recipeDatabase.Length); 
        
        InitializeGameClientRpc(recipeIndex);
        if(updateChefs)
            InitializeChefs();
        SceneManager.Instance.ChangeScene(false, true);
        SceneManager.Instance.RepositionPlayersMarketSceneServerRpc();
    }

    [ClientRpc]
    public void InitializeGameClientRpc(int recipeIndex)
    {
        GetInitialRecipe(recipeIndex);
    }

    void GetInitialRecipe(int recipeIndex)
    {
        targetRecipe = recipeDatabase.ElementAt(recipeIndex);
        GameInterfaceManager.Instance.recipeName.text = targetRecipe.name;

        if (recipeSteps.Count > 0)
        {
            for (int i = 0; i < recipeSteps.Count; i++)
            {
                Destroy(recipeSteps[i].gameObject);
            }
            recipeSteps.Clear();
        }
        
        foreach (var step in ExtractRecipeSteps(targetRecipe))
        {
            step.transform.SetParent(GameInterfaceManager.Instance.recipeSteps.transform);
            Debug.Log("Quantidade de processos e " + step);
        }
    }
    
    public void InitializeChefs()
    {
        List<int> selectedChefsIndexes = new List<int>();

        
        while (selectedChefsIndexes.Count < CHEFS_AMOUNT)
        {
            int index = Random.Range(0, chefsDatabase.Length);
            if (!selectedChefsIndexes.Contains(index))
            {
                selectedChefsIndexes.Add(index);
            }
        }

        
        InitializeChefsClientRpc(selectedChefsIndexes.ToArray());
    }

    [ClientRpc]
    public void InitializeChefsClientRpc(int[] chefIndexes)
    {
        if (chefsGO.Count > 0)
        {
            for (int i = chefsGO.Count - 1; i >= 0; i--)
            {
                Destroy(chefsGO[i]);
            }
        }
        chefs = new List<ChefSO>();
        chefsGO = new List<GameObject>();

        for (int i = 0; i < chefIndexes.Length; i++)
        {
            ChefSO chef = chefsDatabase[chefIndexes[i]];
            chefs.Add(chef);


            GameObject chefGO = Instantiate(chef.prefab, chefsSpawn[i]);
            chefsGO.Add(chefGO);


            foreach (var reviewCondition in chef.conditions)
            {
                foreach (var foodPreference in reviewCondition.foods)
                {
                    chefGO.GetComponent<Dialogue>().dialogueText.Add(foodPreference.name);
                }
            }
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
        var step = Instantiate(GameInterfaceManager.Instance.horizontalGroupPrefab);
        recipeSteps.Add(step);
        if (ingredients != null && ingredients.foods != null)
        {
            for (int i = 0; i < ingredients.foods.Count; i++)
            {
                var ingredient = ingredients.foods[i];
                var item = Instantiate(GameInterfaceManager.Instance.imagePrefab, step.transform);
                var image = item.GetComponent<Image>();
                image.sprite = ingredient.imageFood; 
                image.preserveAspect = true;

                
                if (i < ingredients.foods.Count - 1)
                {
                    var plus = Instantiate(GameInterfaceManager.Instance.imagePrefab, step.transform);
                    var plusImage = plus.GetComponent<Image>();
                    plusImage.sprite = GameInterfaceManager.Instance.plusSprite;
                    plusImage.preserveAspect = true;
                }
            }
        }

        
        if (bench != null)
        {
            var arrow = Instantiate(GameInterfaceManager.Instance.imagePrefab, step.transform);
            var arrowImage = arrow.GetComponent<Image>();
            arrowImage.sprite = GameInterfaceManager.Instance.arrowSprite; 
            arrowImage.preserveAspect = true;

            var benchItem = Instantiate(GameInterfaceManager.Instance.imagePrefab, step.transform);
            var benchImage = benchItem.GetComponent<Image>();

            if(bench.benchType == BenchType.Mortar)
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchOven; 
                benchImage.preserveAspect = true;
            }
            else if (bench.benchType == BenchType.BusenBurner)
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchStove;
                benchImage.preserveAspect = true;
            }
            else
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchBoard;
                benchImage.preserveAspect = true;
            }
            
        }

        
        var equals = Instantiate(GameInterfaceManager.Instance.imagePrefab, step.transform);
        var equalsImage = equals.GetComponent<Image>();
        equalsImage.sprite = GameInterfaceManager.Instance.equalsSprite;
        equalsImage.preserveAspect = true;

        
        var result = Instantiate(GameInterfaceManager.Instance.imagePrefab, step.transform);
        var resultImage = result.GetComponent<Image>();
        resultImage.sprite = recipe.imageFood; 
        resultImage.preserveAspect = true;

        yield return step;
        //yield return horizontalGroupPrefab; //string.Join(" + ", ingredients.foods.Select(f => f.name)) + " -> " + bench.benchType + " = " + recipe.name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyPlayersServerRpc(int playerID, bool isOn){
        AttToggleClientRpc(playerID, isOn);
        //LeaderboardManager.Instance.CanNextRound();
    }

    [ClientRpc]
    public void AttToggleClientRpc(int playerID, bool isOn){
        //GameInterfaceManager.Instance.UpdateToggle(playerID, isOn);
    }

    [ClientRpc]
    public void OnReturnMarketClientRpc(){
        GameInterfaceManager.Instance.clock.SetActive(false);
        GameInterfaceManager.Instance.recipeSteps.SetActive(false);

        var r = GameInterfaceManager.Instance.recipeSteps.GetComponentsInChildren<HorizontalLayoutGroup>();
        foreach (HorizontalLayoutGroup layoutGroup in r)
        {
            Destroy(layoutGroup.gameObject);
        }
        //Reset();
    }

    //[ClientRpc]
    //public void OnPlayerEliminatedClientRpc(int playerID){
    //    Debug.Log("Player eliminado é: "+ playerID);
    //    Reset();
    //    foreach (Player player in FindObjectsOfType<Player>())
    //    {
    //        if (player.id == playerID)
    //        {
    //            player.GetComponent<NetworkObject>().gameObject.SetActive(false);
    //        }
    //    }
    //    numberPlayer--;
    //    //PlayerNetworkManager.Instance.GetPlayerByIndex(playerID).gameObject.SetActive(false);
    //}

    public void OnReturnMarket(){

        OnReturnMarketClientRpc();
        CurrentRound++;
        GameInterfaceManager.Instance.ResetGameHUD();
        //OnPlayerEliminatedClientRpc(EliminationPlayer.Instance.PlayerElimination());
    }

    public void EndGame(){
        EndGameClientRpc();
        LobbyManager.Instance.CloseLobby();
    }
    [ClientRpc]
    void EndGameClientRpc()
    {
        SceneLoader.Instance.LoadLevel(SceneLoader.Scenes.Menu);
    }
    public void ChangeBenchColor(Material material, int playerIndex)
    {
        Material[] list = 
        {
            benchColorRenderer[playerIndex].material,
            material
        };
        benchColorRenderer[playerIndex].materials = list;
    }
    public void Reset(){
        foreach(Interactable objectScene in FindObjectsOfType<Interactable>()){
            if((objectScene as Tool) != null )//&&(objectScene as Tool).isHandTool
                objectScene.DestroySelf();
            if((objectScene as Ingredient) != null )//&&(objectScene as Ingredient).isHandIngredient)
                if(objectScene.transform.parent == null) objectScene.DestroySelf();
        }
        /*for(int i=0; i<numberPlayer; i++){
            NetworkManagerUI.Instance.playerFinalCheck[i].isOn = false;
            NetworkManagerUI.Instance.playerUI[i].gameObject.SetActive(false);
        }*/
        foreach(StorageController store in FindObjectsOfType<StorageController>()){
            store.storageItems.Clear();
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