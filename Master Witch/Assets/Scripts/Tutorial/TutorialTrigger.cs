using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] Interactable interactable;
    [SerializeField] UnityEvent<GameObject, bool> action;
    [SerializeField] bool usePick;
    // Start is called before the first frame update
    void Start()
    {
        interactable.action += OnInteract;
    }

    void OnInteract(bool pick)
    {
        action?.Invoke(gameObject, pick);
    }
}
