using Game.SceneGame;
using Game.SO;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TutorialController : Singleton<TutorialController>
{
    public const int TUTORIAL_TIMER = int.MaxValue;
    [Header("Presentation")]
    [SerializeField, TextArea(5, 10)] string[] tutorialChefMessages;
    [Header("Step 01")]
    [SerializeField] MarketBench[] ingredientsCrates;
    [SerializeField, TextArea(5, 10)] string[] step01Messages;
    [Header("Step 02")]
    [SerializeField, TextArea(5, 10)] string[] startMainMessages;
    [SerializeField, TextArea(5, 10)] string[] step02Messages;
    [SerializeField, TextArea(5, 10)] string[] deliveryMessages;
    [SerializeField, TextArea(5, 10)] string[] endGameMessages;
    [SerializeField] BenchType[] benchSteps;
    [SerializeField] FoodSO[] foodSteps;

    [SerializeField] GameObject tutorialCompassPrefab;
    GameObject compass;
    [SerializeField] TutorialStep[] tutorialSteps;
    int currentStepIndex;
    TutorialStep CurrentStep
    {
        get
        {
            if (currentStepIndex < tutorialSteps.Length)
                return tutorialSteps[currentStepIndex];
            else 
                return null;
        }
    }
    public string[] ChefMessages => tutorialChefMessages;

    private void Start()
    {
        for (int i = 1; i < ingredientsCrates.Length; i++)
        {
            ingredientsCrates[i].gameObject.SetActive(false);
        }
        var player = PlayerNetworkManager.Instance.GetPlayerByIndex(0);
        compass = Instantiate(tutorialCompassPrefab, player.transform.position, Quaternion.identity, player.transform);
    }
    private void Update()
    {
        if (CurrentStep != null)
        {
            var target = new Vector3(CurrentStep.TargetTriggers[0].transform.position.x, compass.transform.position.y, CurrentStep.TargetTriggers[0].transform.position.z);
            compass.transform.LookAt(target);
        }

    }
    public void CheckCurrentStep(GameObject gameObject, bool isPick)
    {
        if (currentStepIndex >= tutorialSteps.Length) return;
        if (CurrentStep.CheckStep(gameObject, isPick))
        {
            CurrentStep.OnInteract();
            currentStepIndex++;
            if(CurrentStep.StartInsta)
                CurrentStep.OnStart();

        }
    }
    public void EndRound()
    {
        SceneManager.Instance.timeCount.Value = 1;

    }
    public void OnStartMain()
    {
        CurrentStep.OnStart();
    }

    public void NextIngredientPickStep()
    {
        var correctStep = false;
        foreach (var item in SceneManager.Instance.benchStorage[0].ingredients)
        {
            correctStep = item.TargetFood == ingredientsCrates[currentStepIndex].food;
            if (correctStep) break;
        }
        if (!correctStep) return;
        StartCoroutine(StartMarketDialogue());
        ingredientsCrates[currentStepIndex].gameObject.SetActive(false);
        currentStepIndex++;
        if (currentStepIndex >= ingredientsCrates.Length) return;
        ingredientsCrates[currentStepIndex].gameObject.SetActive(true);
    }
    IEnumerator StartMarketDialogue()
    {
        DialogueSystem.Instance.chefName.text = GameManager.Instance.chefsGO[0].name.Replace("(Clone)", "").Trim();
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(step01Messages[currentStepIndex]));
        SceneManager.Instance.isMovementAllowed.Value = true;
        if (currentStepIndex >= ingredientsCrates.Length)
            SceneManager.Instance.timeCount.Value = 1;
    }
    IEnumerator StartMainDialogue()
    {
        DialogueSystem.Instance.chefName.text = GameManager.Instance.chefsGO[0].name.Replace("(Clone)", "").Trim();
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(startMainMessages));
        SceneManager.Instance.isMovementAllowed.Value = true;
    }

    public void NextBenchStep(GameObject benchObj, bool isPick)
    {
        var bench = benchObj.GetComponent<Bench>();
        if (bench == null)
        {
            var trialBench = benchObj.GetComponent<TrialBench>();
            if (trialBench == null)
                return;
            if (trialBench.Delivery == GameManager.Instance.TargetRecipe)
            {
                StartCoroutine(EndGameDialogue());
            }
        }
        if (!isPick)
        {
            if (currentStepIndex >= foodSteps.Length) return;
            if (currentStepIndex >= 3 && currentStepIndex < 6)
            {
                if (bench.benchType != BenchType.Cauldron || (bench.ingredients[0].targetFood != foodSteps[3] &&
                    bench.ingredients[0].targetFood != foodSteps[4] && bench.ingredients[0].targetFood != foodSteps[5]))
                {
                    StartCoroutine(DialogueSystem.Instance.StartDialogue($"Quase lá! Tente adicionar todos os ingredientes que preparou no {benchSteps[currentStepIndex]}"));
                    return;
                }
                else if(currentStepIndex >= 5)
                    StartCoroutine(Step02Dialogue());
            }
            else if (bench.benchType != benchSteps[currentStepIndex] || bench.ingredients[0].targetFood != foodSteps[currentStepIndex])
            {
                StartCoroutine(DialogueSystem.Instance.StartDialogue($"Boa! Mas não é o que precisamos. Tente usar o {foodSteps[currentStepIndex].name} na bancada de {benchSteps[currentStepIndex]}"));
                return;
            }
            else
                StartCoroutine(Step02Dialogue());
            currentStepIndex++;
        }
        else
        {
            if (bench.benchType == benchSteps[currentStepIndex])
            {
                StartCoroutine(OnRetrievePotionDialogue());
            }
        }
    }
    public IEnumerator StartDialogue(string[] messages)
    {
        if (messages.Length <= 0) yield break;
        DialogueSystem.Instance.chefName.text = GameManager.Instance.chefsGO[0].name.Replace("(Clone)", "").Trim();
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(messages));
        SceneManager.Instance.isMovementAllowed.Value = true;
    }
    IEnumerator Step02Dialogue()
    {
        DialogueSystem.Instance.chefName.text = GameManager.Instance.chefsGO[0].name.Replace("(Clone)", "").Trim();
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(step02Messages[currentStepIndex]));
        SceneManager.Instance.isMovementAllowed.Value = true;
    }
    IEnumerator OnRetrievePotionDialogue()
    {
        DialogueSystem.Instance.chefName.text = GameManager.Instance.chefsGO[0].name.Replace("(Clone)", "").Trim();
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(deliveryMessages));
        SceneManager.Instance.isMovementAllowed.Value = true;
    }
    IEnumerator EndGameDialogue()
    {
        DialogueSystem.Instance.chefName.text = GameManager.Instance.chefsGO[0].name.Replace("(Clone)", "").Trim();
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(endGameMessages));
        SceneManager.Instance.isMovementAllowed.Value = true;
        SceneManager.Instance.timeCount.Value = 1;
    }

    [Serializable]
    public class TutorialStep
    {
        [SerializeField] string name;
        [SerializeField] InteractionType interactionType;
        [SerializeField] bool startInsta = true;
        [SerializeField, TextArea(5, 10)] string[] startMessages;
        [SerializeField, TextArea(5, 10)] string[] interactMessages;
        [SerializeField, TextArea(5, 10)] string[] errorMessages;
        [SerializeField] TutorialTrigger[] targetTriggers;
        [SerializeField] FoodSO[] foodSteps;
        [SerializeField] ToolsSO[] toolSteps;
        [SerializeField] UnityEvent onStart;
        [SerializeField] UnityEvent onInteract;

        public TutorialTrigger[] TargetTriggers => targetTriggers;
        public bool StartInsta => startInsta;
        public void OnStart() => Instance.StartCoroutine(OnStartCoroutine());
        IEnumerator OnStartCoroutine()
        {
            yield return Instance.StartCoroutine(Instance.StartDialogue(startMessages));
            onStart?.Invoke();
        }
        public void OnInteract() => Instance.StartCoroutine(OnInteractCoroutine());
        IEnumerator OnInteractCoroutine()
        {
            yield return Instance.StartCoroutine(Instance.StartDialogue(interactMessages));
            onInteract?.Invoke();
        }
        public bool CheckStep(GameObject triggerObj, bool isPick)
        {
            var trigger = triggerObj.GetComponent<TutorialTrigger>();
            var bench = triggerObj.GetComponent<Bench>();
            bool confirm = false;
            if (trigger != null && targetTriggers.Contains(trigger))
            {
                confirm = true;
                switch (interactionType)
                {
                    case InteractionType.Pick:
                        if (isPick)
                        {
                            var player = PlayerNetworkManager.Instance.GetPlayerByIndex(0);
                            if (foodSteps.Length > 0)
                            {
                                var ingredient = player.GetComponentInChildren<Ingredient>();
                                if (ingredient == null || !foodSteps.Contains(ingredient.food))
                                    confirm = false;
                            }
                            else if (toolSteps.Length > 0)
                            {
                                var tool = player.GetComponentInChildren<Tool>();
                                if (tool == null || !toolSteps.Contains(tool.tool))
                                    confirm = false;
                            }
                        }
                        else
                            confirm = false;
                        break;
                    case InteractionType.Drop:
                        if (!isPick)
                        {
                            if (foodSteps.Length > 0)
                            {
                                foreach (var targetIngredient in foodSteps)
                                {
                                    if (bench == null)
                                    {
                                        var trialBench = triggerObj.GetComponent<TrialBench>();
                                        if (trialBench != null)
                                        {
                                            if (trialBench.Delivery != targetIngredient)
                                                confirm = false;
                                        }
                                    }
                                    else
                                    {
                                        if (!bench.ingredients.Any(x => x.targetFood == targetIngredient))
                                        {
                                            confirm = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            else if (toolSteps.Length > 0)
                            {
                                var cauldron = bench as Cauldron;
                                foreach (var targetTool in toolSteps)
                                {
                                    if (!cauldron._toolInBench.Contains(targetTool))
                                    {
                                        confirm = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                            confirm = false;
                        break;
                    case InteractionType.Both:
                        break;
                }
            }
            if (!confirm)
            {
                Instance.StartCoroutine(Instance.StartDialogue(errorMessages));
            }
            return confirm;
        }

        enum InteractionType
        {
            Pick,
            Drop,
            Both
        }
    }
}
