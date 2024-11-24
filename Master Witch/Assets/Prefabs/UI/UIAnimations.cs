using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIAnimations : MonoBehaviour
{
    
    public RectTransform recipeStepsTransform;
    public GameObject a,b;
    
    void Start()
    {
        StartButtonPulse(b.transform);
    }

    public void Suspend(InputAction.CallbackContext context)
    {
        if (context.performed)
        recipeStepsTransform.DOAnchorPosY(340, 1.5f).OnComplete(()=> 
        {
            b.SetActive(false); 
            a.SetActive(true);
            StartButtonPulse(a.transform);
        });
    }
    public void Display(InputAction.CallbackContext context)
    {
        if(context.performed)
        recipeStepsTransform.DOAnchorPosY(-375f, 1.5f).OnComplete(()=>
        {
            b.SetActive(true); 
            a.SetActive(false);
            StartButtonPulse(b.transform);
        });
    }
    
    public void StartButtonPulse(Transform interactButton)
    {
        interactButton.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo); 
    }
}
