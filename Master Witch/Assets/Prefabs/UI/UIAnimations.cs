using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIAnimations : MonoBehaviour
{
    private bool isSuspended;
    public RectTransform recipeStepsTransform;
    public GameObject a,b;
    
    void Start()
    {
        StartButtonPulse(b.transform);
    }

    public void Suspend(InputAction.CallbackContext context)
    {
        if (context.performed )
        {
            if(!isSuspended)
            {
                recipeStepsTransform.DOAnchorPosY(340, 1.5f).OnComplete(()=> 
                {
                    isSuspended = true;
                });
                
            }
            else
            {
                recipeStepsTransform.DOAnchorPosY(-375f, 1.5f).OnComplete(()=>
                {
                    
                    isSuspended = false;
                });
                
            }
            
        }
        
    }
    
    public void StartButtonPulse(Transform interactButton)
    {
        interactButton.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo); 
    }
}
