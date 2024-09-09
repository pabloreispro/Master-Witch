using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionWithObjects : MonoBehaviour
{
    public CharacterController controller;
    public float pushForce = 5f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Verifica se o objeto com o qual colidiu tem um Rigidbody
        Rigidbody rb = hit.collider.attachedRigidbody;

        // Se não tiver Rigidbody ou se estiver marcado como kinematic, não faz nada
        if (rb == null || rb.isKinematic)
        {
            return;
        }

        // Não aplicar força se colidir de baixo
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }

        // Aplica força no Rigidbody na direção do impacto
        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }
}

