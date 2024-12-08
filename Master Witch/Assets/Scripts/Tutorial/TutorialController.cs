using Game.SceneGame;
using Game.SO;
using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    int currentStep;
    public string[] ChefMessages => tutorialChefMessages;

    private void Start()
    {
        for (int i = 1; i < ingredientsCrates.Length; i++)
        {
            ingredientsCrates[i].gameObject.SetActive(false);
        }
    }
    public void NextIngredientPickStep()
    {
        var correctStep = false;
        foreach (var item in SceneManager.Instance.benchStorage[0].ingredients)
        {
            correctStep = item.TargetFood == ingredientsCrates[currentStep].food;
            if (correctStep) break;
        }
        if (!correctStep) return;
        StartCoroutine(StartMarketDialogue());
        ingredientsCrates[currentStep].gameObject.SetActive(false);
        currentStep++;
        if (currentStep >= ingredientsCrates.Length) return;
        ingredientsCrates[currentStep].gameObject.SetActive(true);
    }
    IEnumerator StartMarketDialogue()
    {
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(step01Messages[currentStep]));
        SceneManager.Instance.isMovementAllowed.Value = true;
        if (currentStep >= ingredientsCrates.Length)
            SceneManager.Instance.timeCount.Value = 1;
    }
    public void OnStartMain()
    {
        currentStep = 0;
        StartCoroutine(StartMainDialogue());
    }
    IEnumerator StartMainDialogue()
    {
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
            if (currentStep >= foodSteps.Length) return;
            if (currentStep >= 3 && currentStep < 6)
            {
                if (bench.benchType != BenchType.Cauldron || (bench.ingredients[0].targetFood != foodSteps[3] &&
                    bench.ingredients[0].targetFood != foodSteps[4] && bench.ingredients[0].targetFood != foodSteps[5]))
                {
                    StartCoroutine(DialogueSystem.Instance.StartDialogue($"Quase lá! Tente adicionar todos os ingredientes que preparou no {benchSteps[currentStep]}"));
                    return;
                }
                else if(currentStep >= 5)
                    StartCoroutine(Step02Dialogue());
            }
            else if (bench.benchType != benchSteps[currentStep] || bench.ingredients[0].targetFood != foodSteps[currentStep])
            {
                StartCoroutine(DialogueSystem.Instance.StartDialogue($"Boa! Mas não é o que precisamos. Tente usar o {foodSteps[currentStep].name} na bancada de {benchSteps[currentStep]}"));
                return;
            }
            else
                StartCoroutine(Step02Dialogue());
            currentStep++;
        }
        else
        {
            if (bench.benchType == benchSteps[currentStep])
            {
                StartCoroutine(OnRetrievePotionDialogue());
            }
        }
    }
    IEnumerator Step02Dialogue()
    {
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(step02Messages[currentStep]));
        SceneManager.Instance.isMovementAllowed.Value = true;
    }
    IEnumerator OnRetrievePotionDialogue()
    {
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(deliveryMessages));
        SceneManager.Instance.isMovementAllowed.Value = true;
    }
    IEnumerator EndGameDialogue()
    {
        SceneManager.Instance.isMovementAllowed.Value = false;
        yield return StartCoroutine(DialogueSystem.Instance.StartDialogue(endGameMessages));
        SceneManager.Instance.isMovementAllowed.Value = true;
        SceneManager.Instance.timeCount.Value = 1;
    }
}
