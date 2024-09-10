using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionWithObjects : MonoBehaviour
{
    public CharacterController controller;
    public float pushForce = 5f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
        Rigidbody rb = hit.collider.attachedRigidbody;

        
        if (rb == null || rb.isKinematic)
        {
            return;
        }
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }
        
        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }
}

