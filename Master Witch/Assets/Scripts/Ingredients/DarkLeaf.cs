using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DarkLeaf : Ingredient
{
    //Escurece visao
    [SerializeField] LayerMask playerMask;
    [SerializeField] float range;
    [SerializeField] float duration;
    [SerializeField] GameObject fogVfx;
    [SerializeField] SphereCollider aoeCollider;
    List<PlayerMovement> players = new List<PlayerMovement>();
    Volume blindnessVolume;
    public override void StartEffect()
    {
        base.StartEffect();
        blindnessVolume = GameObject.Find("DarkLeafBlindnessVolume").GetComponent<Volume>();
        aoeCollider.enabled = true;
        aoeCollider.radius = range;
        var vfx = Instantiate(fogVfx, transform.position, fogVfx.transform.rotation, transform);
        Destroy(vfx, duration);
        Invoke(nameof(EndEffect), duration);
    }
    public override void EndEffect()
    {
        base.EndEffect();
        aoeCollider.enabled = false;
        blindnessVolume.weight = 0;
    }
    public override void OnEffectCanceled()
    {
        base.OnEffectCanceled();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(blindnessVolume == null)
            blindnessVolume = GameObject.Find("DarkLeafBlindnessVolume").GetComponent<Volume>();
        if (((1 << other.gameObject.layer) & playerMask) != 0)
        {
            var player = other.GetComponent<PlayerMovement>();
            if (player.IsLocalPlayer)
            {
                blindnessVolume.weight = 1;
            }
            players.Add(player);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerMask) != 0)
        {
            var player = other.GetComponent<PlayerMovement>();
            blindnessVolume.weight = 0;
            players.Remove(player);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
