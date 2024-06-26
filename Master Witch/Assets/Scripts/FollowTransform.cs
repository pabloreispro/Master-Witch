using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform targetTransform;

    void Update()
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
        }
    }
}