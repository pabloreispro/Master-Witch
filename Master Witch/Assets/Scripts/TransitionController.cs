using System.Collections;
using System.Collections.Generic;
using UI;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public static class AnimatorType
{
    public const int SceneTransition = 0;
    public const int CountdownMarket = 1;
    public const int CountdownMain = 2;
    public const int RecipeName = 3;
    public const int RecipeSteps = 4;
    
}
public static class PanelType
{
    public const int SceneTransition = 0;
    public const int CountdownMarket = 1;
    public const int CountdownMain = 2;
    public const int RecipeName = 3;
    public const int RecipeSteps = 4;
    
}

public class TransitionController : SingletonNetwork<TransitionController>
{
    public Animator animatorSceneTransition, animatorCountdownMarket, animatorCountdownMain,animatorRN,animatorRS;
    public AnimationClip fadeIn,fadeOut,countdownMarket,countdownMain;
    public GameObject transitionPanel,countdownMarketPanel,countdownMainPanel, recipeNamePanel, recipeStepsPanel;

    #region Gets 
    Animator GetAnimator(int animatorType)
    {
        switch (animatorType)
        {
            case AnimatorType.SceneTransition:
                return animatorSceneTransition;

            case AnimatorType.CountdownMarket:
                return animatorCountdownMarket;  

            case AnimatorType.CountdownMain:
                return animatorCountdownMain;  

            case AnimatorType.RecipeName:
                return animatorRN;

            case AnimatorType.RecipeSteps:
                return animatorRS;

            default:
                return null;
        }
    }

    GameObject GetPanel(int panelType)
    {
        switch (panelType)
        {
            case PanelType.SceneTransition:
                return transitionPanel;

            case PanelType.CountdownMarket:
                return countdownMarketPanel;

            case PanelType.CountdownMain:
                return countdownMainPanel;

            case PanelType.RecipeName:
                return recipeNamePanel;

            case PanelType.RecipeSteps:
                return recipeStepsPanel;

            default:
                return null;
        }
    }

    #endregion

    [ClientRpc]
    void ActivatePanelClientRpc(int panelType, bool isActive)
    {
        GameObject selectedPanel = GetPanel(panelType);
        if (selectedPanel != null)
        {
            selectedPanel.SetActive(isActive);
        }
    }

    [ClientRpc]
    void PlayAnimationClientRpc(int animatorType, string clip)
    {
        Animator selectedAnimator = GetAnimator(animatorType);
        selectedAnimator.Play(clip);
    }

    
    public IEnumerator TransitionMainScene()
    {
        ActivatePanelClientRpc(PanelType.SceneTransition, true);
        PlayAnimationClientRpc(AnimatorType.SceneTransition, fadeIn.name);
        yield return new WaitForSeconds(fadeIn.length);

        SceneManager.Instance.ChangeSceneServerRpc(false,true);
        SceneManager.Instance.RepositionPlayerServerRpc();

        PlayAnimationClientRpc(AnimatorType.SceneTransition, fadeOut.name);
        yield return new WaitForSeconds(fadeOut.length);
        ActivatePanelClientRpc(PanelType.SceneTransition, false);

        ActivatePanelClientRpc(PanelType.CountdownMain, true);
        PlayAnimationClientRpc(AnimatorType.CountdownMain,countdownMain.name);
        yield return new WaitForSeconds(countdownMain.length);
        SceneManager.Instance.StartMain();
        ActivatePanelClientRpc(PanelType.CountdownMain, false);
        
    }

    public IEnumerator TransitionMarketScene()
    {
        ActivatePanelClientRpc(PanelType.SceneTransition, true);

        PlayAnimationClientRpc(AnimatorType.SceneTransition,fadeIn.name);
        yield return new WaitForSeconds(fadeIn.length);

        SceneManager.Instance.ChangeSceneServerRpc(true,false);
        SceneManager.Instance.RepositionPlayerServerRpc();

        PlayAnimationClientRpc(AnimatorType.SceneTransition,fadeOut.name);
        yield return new WaitForSeconds(fadeOut.length);
        ActivatePanelClientRpc(PanelType.SceneTransition, false);
        
        ActivatePanelClientRpc(PanelType.CountdownMarket, true);
        PlayAnimationClientRpc(AnimatorType.CountdownMarket,countdownMarket.name);
        yield return new WaitForSeconds(countdownMarket.length);
        SceneManager.Instance.StartMarket();
        ActivatePanelClientRpc(PanelType.CountdownMarket, false);
        

    }

    
}