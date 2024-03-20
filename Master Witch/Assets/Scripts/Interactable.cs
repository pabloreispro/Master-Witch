using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public abstract class Interactable : NetworkBehaviour
{
    
    public virtual void Drop(GameObject item){}
    public virtual void Pick(GameObject item){}
}
