using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class CameraController : Singleton<CameraController>
{
    public Transform target;
    public Transform initialPosition;
    public CinemachineFreeLook virtualCamera;
    
    void Start()
    {
        virtualCamera = GetComponent<CinemachineFreeLook>();
        initialPosition = virtualCamera.transform;
    }
    void LateUpdate()
    {
        if(target == null)
        {
            virtualCamera.transform.position =  initialPosition.position;
            virtualCamera.transform.rotation = initialPosition.rotation;
        }
        else
        {
            virtualCamera.LookAt = target;
        }
    }

    
}

