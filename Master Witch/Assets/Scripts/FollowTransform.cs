using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform targetTransform; // Transform que você quer seguir

    void Update()
    {
        if (targetTransform != null)
        {
            // Atualiza posição e rotação baseado no targetTransform
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
        }
    }
}