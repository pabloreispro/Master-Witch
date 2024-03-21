using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public abstract class Interactable : MonoBehaviour
{
    
    public virtual void Drop(Player player){}
    public virtual void Pick(Player player){}
}
