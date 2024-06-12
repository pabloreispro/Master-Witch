using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockTimer : MonoBehaviour
{
    public Transform clockHand; 
    public float maxTime = 0;  
    private float currentTime;  
    

    void Start()
    {
        maxTime = SceneManager.Instance.timeCount.Value;
        currentTime = maxTime; 
        
    }

    void Update()
    {
        if (currentTime > 0)
        {
            currentTime = SceneManager.Instance.timeCount.Value;

            if (currentTime <= 0)
            {
                currentTime = 0;
            }

            float angle = (currentTime / maxTime) * 360; 
            clockHand.eulerAngles = new Vector3(0, 0, angle);

           
        }
    }

   
}
