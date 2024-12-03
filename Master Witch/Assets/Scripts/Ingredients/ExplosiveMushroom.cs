using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveMushroom : Ingredient
{
    //Explode e empurra tudo e todos
    [SerializeField] LayerMask targetMask;
    [SerializeField] float range;
    [SerializeField] float force;
    [SerializeField] GameObject fogVfx;
    public override void StartEffect()
    {
        base.StartEffect();
        var vfx = Instantiate(fogVfx, transform.position, fogVfx.transform.rotation, transform);
        Destroy(vfx, 2);
        var targets = Physics.OverlapSphere(transform.position, range, targetMask);
        foreach (var target in targets)
        {
            var player = target.GetComponent<PlayerMovement>();
            if(player != null)
                target.GetComponent<PlayerMovement>().AddExplosiveForce(force, transform.position, range, 5, 0.5f);
            else
                target.GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, range, 5);

        }
        Invoke(nameof(EndEffect), 2);
    }
    public override void EndEffect()
    {
        base.EndEffect();
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
