using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform targetTransform;

    void LateUpdate()
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
        }
    }
}