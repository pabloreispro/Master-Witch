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
    [SerializeField] MeshRenderer[] benchColorRenderer;
    [SerializeField] RecipeSO[] recipeDatabase;
    [SerializeField] ChefSO[] chefsDatabase;
    [SerializeField] Transform[] chefsSpawn;
  
    public Text RecipeText;
    
    Dictionary<int, float> ResultFinal = new Dictionary<int, float>();
    RecipeSO targetRecipe;
    public float matchStartTime;
    public List<ChefSO> chefs;
    public List<GameObject> chefsGO;
    
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
        NewCamController.Instance.Intro();
        NetworkManagerUI.Instance.OnGameStartedClientRpc();
        /*for (int i = 0; i < benches.Length; i++)
        {
            var player = PlayerNetworkManager.Instance.GetPlayerByIndex(i);
            if (player != null)
                benches[i].SetPlayer(player);
            else break;
        }*/
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

    [ServerRpc]
    public void InitializeGameServerRpc()
    {
        int recipeIndex = Random.Range(0, recipeDatabase.Length); 
        InitializeGameClientRpc(recipeIndex); 
        
    }

    [ClientRpc]
    public void InitializeGameClientRpc(int recipeIndex)
    {
        GetInitialRecipe(recipeIndex);
        SelectChefs();
        
    }

    void GetInitialRecipe(int recipeIndex)
    {
        targetRecipe = recipeDatabase.ElementAt(recipeIndex);
        NetworkManagerUI.Instance.recipeName.text = targetRecipe.name;
        
        foreach (var step in ExtractRecipeSteps(targetRecipe))
        {
            step.transform.SetParent(NetworkManagerUI.Instance.recipeSteps.transform);
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
            GameObject chefGO = Instantiate(chef.prefab,chefsSpawn[i]);
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
    public void QuitGame()
    {
        Application.Quit();
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
        var step = Instantiate(NetworkManagerUI.Instance.horizontalGroupPrefab);
        
        if (ingredients != null && ingredients.foods != null)
        {
            for (int i = 0; i < ingredients.foods.Count; i++)
            {
                var ingredient = ingredients.foods[i];
                var item = Instantiate(NetworkManagerUI.Instance.imagePrefab, step.transform);
                var image = item.GetComponent<Image>();
                image.sprite = ingredient.imageFood; 
                image.preserveAspect = true;

                
                if (i < ingredients.foods.Count - 1)
                {
                    var plus = Instantiate(NetworkManagerUI.Instance.imagePrefab, step.transform);
                    var plusImage = plus.GetComponent<Image>();
                    plusImage.sprite = NetworkManagerUI.Instance.plusSprite;
                    plusImage.preserveAspect = true;
                }
            }
        }

        
        if (bench != null)
        {
            var arrow = Instantiate(NetworkManagerUI.Instance.imagePrefab, step.transform);
            var arrowImage = arrow.GetComponent<Image>();
            arrowImage.sprite = NetworkManagerUI.Instance.arrowSprite; 
            arrowImage.preserveAspect = true;

            var benchItem = Instantiate(NetworkManagerUI.Instance.imagePrefab, step.transform);
            var benchImage = benchItem.GetComponent<Image>();

            if(bench.benchType == BenchType.Mortar)
            {
                benchImage.sprite = NetworkManagerUI.Instance.benchOven; 
                benchImage.preserveAspect = true;
            }
            else if (bench.benchType == BenchType.BusenBurner)
            {
                benchImage.sprite = NetworkManagerUI.Instance.benchStove;
                benchImage.preserveAspect = true;
            }
            else
            {
                benchImage.sprite = NetworkManagerUI.Instance.benchBoard;
                benchImage.preserveAspect = true;
            }
            
        }

        
        var equals = Instantiate(NetworkManagerUI.Instance.imagePrefab, step.transform);
        var equalsImage = equals.GetComponent<Image>();
        equalsImage.sprite = NetworkManagerUI.Instance.equalsSprite;
        equalsImage.preserveAspect = true;

        
        var result = Instantiate(NetworkManagerUI.Instance.imagePrefab, step.transform);
        var resultImage = result.GetComponent<Image>();
        resultImage.sprite = recipe.imageFood; 
        resultImage.preserveAspect = true;

        yield return step;
        //yield return horizontalGroupPrefab; //string.Join(" + ", ingredients.foods.Select(f => f.name)) + " -> " + bench.benchType + " = " + recipe.name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyPlayersServerRpc(int playerID, bool isOn){
        AttToggleClientRpc(playerID, isOn);
        EndRound.Instance.CanNextRound();
    }

    [ClientRpc]
    public void AttToggleClientRpc(int playerID, bool isOn){
        NetworkManagerUI.Instance.UpdateToggle(playerID, isOn);
    }

    [ClientRpc]
    public void OnReturnMarketClientRpc(){
        NetworkManagerUI.Instance.finalPanel.SetActive(false);
        NetworkManagerUI.Instance.clock.active = false;
        NetworkManagerUI.Instance.recipeSteps.active = false;

        var r = NetworkManagerUI.Instance.recipeSteps.GetComponentsInChildren<HorizontalLayoutGroup>();
        foreach (HorizontalLayoutGroup layoutGroup in r)
        {
            Destroy(layoutGroup.gameObject);
        }
        //Reset();
    }

    [ClientRpc]
    public void OnPlayerEliminatedClientRpc(int playerID){
        Debug.Log("Player eliminado é: "+ playerID);
        Reset();
        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.id == playerID)
            {
                player.GetComponent<NetworkObject>().gameObject.SetActive(false);
            }
        }
        numberPlayer--;
        //PlayerNetworkManager.Instance.GetPlayerByIndex(playerID).gameObject.SetActive(false);
    }

    public void OnReturnMarket(){
        OnReturnMarketClientRpc();
        OnPlayerEliminatedClientRpc(EliminationPlayer.Instance.PlayerElimination());
    }

    public void EndGame(){
        EndGameClientRpc();
        LobbyManager.Instance.CloseServer();
    }
    [ClientRpc]
    void EndGameClientRpc()
    {
        NetworkManagerUI.Instance.EnableMenu();
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
                objectScene.DestroySelf();
        }
        for(int i=0; i<numberPlayer; i++){
            NetworkManagerUI.Instance.playerFinalCheck[i].isOn = false;
            NetworkManagerUI.Instance.playerUI[i].gameObject.SetActive(false);
        }
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