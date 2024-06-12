using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Network;
using System.Linq;
using Unity.VisualScripting;
using UI;
using TMPro;
using UnityEditorInternal;
public class AnimationsController : SingletonNetwork<AnimationsController>
{
    public Animator animatorChangeScene;
    public AnimationClip transitionChangeScene;
    public GameObject a;
    void Start()
    {
        
    }

    

    [ServerRpc (RequireOwnership = false)]
    public void TransitionSceneServerRpc()
    {
        TransitionSceneClientRpc();
    }

    [ClientRpc]
    public void TransitionSceneClientRpc()
    {
        StartCoroutine(TransitionScene());
        
    }

    public IEnumerator TransitionScene()
    {
        
        a.SetActive(true);
        yield return new WaitForSeconds(transitionChangeScene.length);
        a.SetActive(false);
    }
}
