using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual Interactable Drop(Interactable item){return null;}
    public virtual Interactable Pick(Interactable item){return null;}
}
