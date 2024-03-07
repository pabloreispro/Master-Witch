using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool isBench;
    public bool isIngredient;
    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if(hit.gameObject.CompareTag("Bench")){
            Bench.instance.foodAsset = gameObject.transform.GetChild(0).gameObject;
        }
        if(hit.gameObject.CompareTag("Ingredient")){
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            
        }
    }
}
