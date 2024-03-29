using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;
using Game.SO;

public class Player : NetworkBehaviour
{
    public int id;
    string name;
    Color color;
    public Interactable interact;
    public GameObject assetIngredient;
    public FoodSO ingredient;
    public bool stateObject;
    public bool isHandfull;
    public NetworkVariable<bool> stateObjectIngrediente = new NetworkVariable<bool>();
    public NetworkVariable<bool> isHand = new NetworkVariable<bool>();

    public void ONetworkSpawn()
    {
        stateObjectIngrediente.Value = false;
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if(hit.gameObject.GetComponent<Interactable>() != null){
            interact = hit.gameObject.GetComponent<Interactable>();
        }
    }
}
