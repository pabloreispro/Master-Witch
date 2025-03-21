using System.Collections;
using System.Collections.Generic;
using UI;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using Game.SceneGame;
public static class AnimatorType
{
    public const int SceneTransition = 0;
    public const int CountdownMarket = 1;
    public const int CountdownMain = 2;
    public const int RecipeInitial = 3;
    
    
}
public static class PanelType
{
    public const int SceneTransition = 0;
    public const int CountdownMarket = 1;
    public const int CountdownMain = 2;
    public const int RecipeInitial = 3;
    
}

public class TransitionController : SingletonNetwork<TransitionController>
{
    public Animator animatorSceneTransition, animatorCountdownMarket, animatorCountdownMain,animatorRecipe;
    public AnimationClip fadeIn,fadeOut,countdownMarket,countdownMain,recipeTransition;
    public GameObject transitionPanel,countdownMarketPanel,countdownMainPanel, recipeInitialPanel;

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

           case PanelType.RecipeInitial:
                return animatorRecipe;


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

            case PanelType.RecipeInitial:
                return recipeInitialPanel;


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
        
        SceneManager.Instance.isMovementAllowed.Value = false;
        NewCamController.Instance.minXHorizontal.Value = -46;
        NewCamController.Instance.maxXHorizontal.Value = 46;
        NewCamController.Instance.minZ.Value = -35.5f;
        NewCamController.Instance.maxZ.Value = 1;
        /*
        ActivatePanelClientRpc(PanelType.SceneTransition, true);
        PlayAnimationClientRpc(AnimatorType.SceneTransition, fadeIn.name);
        yield return new WaitForSeconds(fadeIn.length);
        */
        ActivatePanelClientRpc(PanelType.SceneTransition, true);
        PlayAnimationClientRpc(AnimatorType.SceneTransition,fadeIn.name);
        yield return new WaitForSeconds(2f);

        SceneManager.Instance.ChangeSceneServerRpc(false,true);
        SceneManager.Instance.RepositionPlayersMainSceneServerRpc();
        SceneManager.Instance.RepositionStorageMainSceneServerRpc();
        yield return new WaitForSeconds(1f);

        PlayAnimationClientRpc(AnimatorType.SceneTransition,fadeOut.name);
        yield return new WaitForSeconds(fadeOut.length);
        ActivatePanelClientRpc(PanelType.SceneTransition, false);
        /*
        PlayAnimationClientRpc(AnimatorType.SceneTransition, fadeOut.name);
        yield return new WaitForSeconds(fadeOut.length);
        ActivatePanelClientRpc(PanelType.SceneTransition, false);*/

        ActivatePanelClientRpc(PanelType.CountdownMain, true);
        PlayAnimationClientRpc(AnimatorType.CountdownMain,countdownMain.name);
        yield return new WaitForSeconds(countdownMain.length);

        SceneManager.Instance.StartMain();
        SceneManager.Instance.isMovementAllowed.Value = true;
        ActivatePanelClientRpc(PanelType.CountdownMain, false);

        yield return new WaitForSeconds(0);
        
    }
    [ClientRpc]
    public void RClientRpc()
    {
        var r = recipeInitialPanel.GetComponentsInChildren<HorizontalLayoutGroup>();
        foreach (HorizontalLayoutGroup layoutGroup in r)
        {
            Destroy(layoutGroup.gameObject);
        }
    }
    public IEnumerator TransitionMarketScene()
    {
        //RClientRpc();
        //GameManager.Instance.InitializeGameServerRpc();
        SceneManager.Instance.isMovementAllowed.Value = false;
        ActivatePanelClientRpc(PanelType.SceneTransition, true);

        PlayAnimationClientRpc(AnimatorType.SceneTransition,fadeIn.name);
        yield return new WaitForSeconds(2f);

        SceneManager.Instance.ChangeSceneServerRpc(true,false);
        SceneManager.Instance.RepositionPlayersMarketSceneServerRpc();
        SceneManager.Instance.RepositionStorageMarketSceneServerRpc();
        yield return new WaitForSeconds(1f);

        PlayAnimationClientRpc(AnimatorType.SceneTransition,fadeOut.name);
        yield return new WaitForSeconds(fadeOut.length);
        ActivatePanelClientRpc(PanelType.SceneTransition, false);
        
        /*ActivatePanelClientRpc(PanelType.RecipeInitial, true);
        PlayAnimationClientRpc(AnimatorType.RecipeInitial, recipeTransition.name);
        yield return new WaitForSeconds(recipeTransition.length);
        ActivatePanelClientRpc(PanelType.RecipeInitial, false);*/

        ActivatePanelClientRpc(PanelType.CountdownMarket, true);
        PlayAnimationClientRpc(AnimatorType.CountdownMarket,countdownMarket.name);
        yield return new WaitForSeconds(countdownMarket.length);

        SceneManager.Instance.StartMarket();
        SceneManager.Instance.isMovementAllowed.Value = true;
        ActivatePanelClientRpc(PanelType.CountdownMarket, false);
        yield return new WaitForSeconds(0); 

    }
    

    
}