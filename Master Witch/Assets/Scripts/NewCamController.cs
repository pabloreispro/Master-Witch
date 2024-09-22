using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NewCamController : SingletonNetwork<NewCamController>
{
    private Transform initialPosition;
    public Transform target; 

    [Header("Movement Configs")]
    [SerializeField] private float speedMovement;
    public NetworkVariable<float> minXHorizontal;
    public NetworkVariable<float> maxXHorizontal;
    public NetworkVariable<float> minZ;
    public NetworkVariable<float> maxZ;
    
    [Header("Rotation Configs")]

    [SerializeField] private float speedRotation;
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    public Vector3 offset;

    void Start()
    {
        initialPosition = this.transform;
    }
    void LateUpdate()
    {
        if (target != null)
        {
            //LookAtTarget();
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
        float boundX = Mathf.Clamp(target.position.x + offset.x, minXHorizontal.Value, maxXHorizontal.Value);
        float boundZ = Mathf.Clamp(target.position.z + offset.z, minZ.Value, maxZ.Value);
        Vector3 targetPosition = new Vector3(boundX,target.position.y+offset.y, boundZ);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, speedMovement * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}
