using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Interactable))]
public class TutorialTrigger : MonoBehaviour
{
    void Start()
    {
        GetComponent<Interactable>().action += OnInteract;
    }

    void OnInteract(bool pick)
    {
        TutorialController.Instance.CheckCurrentStep(gameObject, pick);
    }
}
