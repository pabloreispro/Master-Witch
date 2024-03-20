using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;

public class Player : NetworkBehaviour
{
    public static Player instance;
    public int id;
    string name;
    Color color;
    public Interactable interact;
    public bool isHandFull;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        
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
