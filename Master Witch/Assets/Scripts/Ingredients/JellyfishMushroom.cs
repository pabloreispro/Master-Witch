using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyfishMushroom : Ingredient
{
    //Choque a cada pequenos intervalos, travando movimento e dropando itens
    [SerializeField] LayerMask playerMask;
    [SerializeField] float range;
    [SerializeField] float duration;
    [SerializeField] float stunDuration;
    [SerializeField] float delay;
    [SerializeField] GameObject rootVfx;
    PlayerMovement[] players;
    public override void StartEffect()
    {
        base.StartEffect();
        var vfx = Instantiate(rootVfx, transform.position, rootVfx.transform.rotation);
        Destroy(vfx, duration);
        InvokeRepeating(nameof(ApplyStun), 0, delay);
        Invoke(nameof(EndEffect), duration);
    }
    void ApplyStun()
    {
        var playersColliders = Physics.OverlapSphere(transform.position, range, playerMask);
        players = new PlayerMovement[playersColliders.Length];
        for (int i = 0; i < playersColliders.Length; i++)
        {
            players[i] = playersColliders[i].GetComponent<PlayerMovement>();
            players[i].CanMove = false;

            if (players[i].isHand.Value && NetworkManager.IsHost)
            {
                players[i].DropItemHandServerRpc();
            }
        }
        Invoke(nameof(RemoveStun), stunDuration);
    }
    void RemoveStun()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].CanMove = true;
        }
    }
    public override void EndEffect()
    {
        base.EndEffect();
        CancelInvoke();
        RemoveStun();
    }
    public override void OnEffectCanceled()
    {
        base.OnEffectCanceled();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
