using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCamController : Singleton<NewCamController>
{
    private Transform initialPosition;
    public Transform target; 

    [Header("Movement Configs")]
    [SerializeField] private float speedMovement;
    [SerializeField] private float minXHorizontal;
    [SerializeField] private float maxXHorizontal;
    
    [Header("Rotation Configs")]

    [SerializeField] private float speedRotation;
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    void Start()
    {
        initialPosition = this.transform;
    }
    void LateUpdate()
    {
        if (target != null)
        {
            LookAtTarget();
            FollowTarget(); 
        }
        else
        {
            transform.position = initialPosition.position;
        }
    }

    void LookAtTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        float xRotation = Mathf.Clamp(targetRotation.eulerAngles.x, minAngle, maxAngle);
        Quaternion newTargetRotation = Quaternion.Euler(new Vector3(xRotation, transform.eulerAngles.y, targetRotation.eulerAngles.z));
        
        transform.rotation = Quaternion.Slerp(transform.rotation, newTargetRotation, speedRotation * Time.deltaTime);
    }

    void FollowTarget()
    {
        float boundX = Mathf.Clamp(target.position.x, minXHorizontal, maxXHorizontal);
        Vector3 targetPosition = new Vector3(boundX, transform.position.y, transform.position.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, speedMovement * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}
