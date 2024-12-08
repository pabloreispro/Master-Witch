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
using UI.Leaderboard;
using Game.SceneGame;

public class GameManager : SingletonNetwork<GameManager>
{
    const int CHEFS_AMOUNT = 3;
    GameState gameState;
    GameMode gameMode;
    [SerializeField] FoodDatabaseSO foodDatabase;
    [SerializeField] Bench[] benches;
    [SerializeField] MeshRenderer[] benchColorRenderer;
    [SerializeField] MeshRenderer[] storageColorRenderer;
    [SerializeField] RecipeSO[] recipeDatabase;
    [SerializeField] ChefSO[] chefsDatabase;
    [SerializeField] Transform[] chefsSpawn;
  
    public Text RecipeText;
    
    RecipeSO targetRecipe;
    public float matchStartTime;
    public List<ChefSO> chefs;
    public List<GameObject> chefsGO;
    List<GameObject> recipeSteps = new List<GameObject>();
    int readyPlayersAmount;
    NetworkVariable<int> currentRound = new NetworkVariable<int>();
    [Header("DEBUG")]
    public bool skipIntro;
    public bool SkipIntro => skipIntro;
    public int TotalRounds => gameMode == GameMode.Tutorial ? 1 : 2;
    #region Properties
    public GameState GameState => gameState;
    public GameMode GameMode => gameMode;
    public FoodDatabaseSO FoodDatabaseSO => foodDatabase;
    public RecipeSO TargetRecipe => targetRecipe;
    public List<ChefSO> Chefs => chefs;
    public int CurrentRound => currentRound.Value;
    #endregion

    //public int numberPlayer;


    void Start()
    {
        if (NetworkManager.IsServer)
        {
            readyPlayersAmount++;
            StartCoroutine(StartGame());
        }
        else
            PlayerLoadedServerRpc();
    }

    public IEnumerator StartGame()
    {
        while (readyPlayersAmount < PlayerNetworkManager.Instance.PlayersCount)
        {
            yield return null;
        }
        readyPlayersAmount = 0;
        Debug.Log("Game Started");
        currentRound.Value = 1;
        InitializeGame();
    }
    [ServerRpc(RequireOwnership = false)]
    void PlayerLoadedServerRpc()
    {
        readyPlayersAmount++;
    }

    [ClientRpc]
    void OnGameStartedClientRpc(bool playIntro)
    {
        if(playIntro)
            NewCamController.Instance.IntroClient(GameMode == GameMode.Tutorial);
        if (GameMode == GameMode.Tutorial)
        {
            SceneManager.Instance.TIMER_MARKET = TutorialController.TUTORIAL_TIMER;
            SceneManager.Instance.TIMER_MAIN = TutorialController.TUTORIAL_TIMER;
        }
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

    public void InitializeGame(bool firstInit = true)
    {
        gameMode = TutorialController.Instance != null ? GameMode.Tutorial : GameMode.Main;
        int recipeIndex = 0;
        if (gameMode != GameMode.Tutorial)
            recipeIndex = Random.Range(0, recipeDatabase.Length);
        InitializeGameClientRpc(recipeIndex, gameMode);
        if (firstInit)
            InitializeChefs();
        SceneManager.Instance.ChangeScene(false, true);
        SceneManager.Instance.RepositionPlayersMarketSceneServerRpc();
        OnGameStartedClientRpc(firstInit);
    }

    [ClientRpc]
    public void InitializeGameClientRpc(int recipeIndex, GameMode gameMode)
    {
        this.gameMode = gameMode;
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
            Debug.Log("Step before " + step.transform.localScale);
            Debug.Log("Step aft " + step.transform.localScale);
            Debug.Log("Quantidade de processos e " + step);
        }
    }
    
    public void InitializeChefs()
    {
        List<int> selectedChefsIndexes = new List<int>();

        var amount = GameMode == GameMode.Main ? CHEFS_AMOUNT : 1;
        while (selectedChefsIndexes.Count < amount)
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
        var step = Instantiate(GameInterfaceManager.Instance.horizontalGroupPrefab, GameInterfaceManager.Instance.recipeStepsContent.transform);
        Debug.Log("Instantiatie "+ step.name);
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
                benchImage.sprite = GameInterfaceManager.Instance.benchMortar; 
                benchImage.preserveAspect = true;
            }
            else if (bench.benchType == BenchType.BusenBurner)
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchBusen;
                benchImage.preserveAspect = true;
            }
            else if (bench.benchType == BenchType.Cutting)
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchCutting;
                benchImage.preserveAspect = true;
            }
            else if (bench.benchType == BenchType.Alembic)
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchAlambic;
                benchImage.preserveAspect = true;
            }
            else if (bench.benchType == BenchType.Cauldron)
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchCauldron;
                benchImage.preserveAspect = true;
            }
            else if (bench.benchType == BenchType.Furnace)
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchFurnace;
                benchImage.preserveAspect = true;
            }
            else if (bench.benchType == BenchType.MysteriousFountain)
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchMysterious;
                benchImage.preserveAspect = true;
            }
            else if (bench.benchType == BenchType.Refrigeration)
            {
                benchImage.sprite = GameInterfaceManager.Instance.benchRefrigeration;
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
    public void ReadyPlayersServerRpc(ulong playerNetworkID, bool isReady){
        if(isReady)
            readyPlayersAmount++;
        else 
            readyPlayersAmount--;
        UpdatePlayerReadyClientRpc(PlayerNetworkManager.Instance.GetPlayerIndexID(PlayerNetworkManager.Instance.GetPlayer[playerNetworkID]), isReady);
        CheckNextRound();
    }

    [ClientRpc]
    public void UpdatePlayerReadyClientRpc(int playerID, bool isOn){
        LeaderboardManager.Instance.UpdateReadyPlayers(playerID, isOn);
    }
    //Server-Side
    void CheckNextRound()
    {
        //if (!finalGame)
        //{
        //    //if (PlayerNetworkManager.Instance.PlayersData.Length > 2)
        //    if (CurrentRound < TotalRounds)
        //        ReturnMarket();
        //    else
        //        GameInterfaceManager.Instance.UpdadeScreenFinalClientRpc();
        //}
        //else
        //{
        //    EndGame();
        //}
        if (readyPlayersAmount < PlayerNetworkManager.Instance.PlayersCount) return;
        readyPlayersAmount = 0;
        ResetInfoBetweenRoundsClientRpc();
        if (CurrentRound < TotalRounds)
            ReturnMarket();
        else
            EndGame();
    }
    [ClientRpc]
    void ResetInfoBetweenRoundsClientRpc()
    {
        LeaderboardManager.Instance.ResetInfo();
    }
    public void ReturnMarket()
    {
        OnReturnMarket();
        //NewCamController.Instance.IntroClient();
        GameInterfaceManager.Instance.clock.SetActive(true);
        GameInterfaceManager.Instance.recipeSteps.SetActive(true);
        InitializeGame(false);
        StartCoroutine(TransitionController.Instance.TransitionMarketScene());
    }

    [ClientRpc]
    public void OnReturnMarketClientRpc(){
        GameInterfaceManager.Instance.clock.SetActive(true);
        GameInterfaceManager.Instance.recipeSteps.SetActive(true);

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
        currentRound.Value++;
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
        Material[] benchList = 
        {
            material,
            benchColorRenderer[playerIndex].material
        };
        Material[] storageList = 
        {
            storageColorRenderer[playerIndex].material,
            material,
        };
        benchColorRenderer[playerIndex].materials = benchList;
        storageColorRenderer[playerIndex].materials = storageList;
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
public enum GameMode
{
    Main,
    Tutorial
}