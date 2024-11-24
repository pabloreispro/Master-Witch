using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : Singleton<TutorialController>
{
    [SerializeField, TextArea(5, 10)] string[] tutorialChefMessages;
    public string[] ChefMessages => tutorialChefMessages;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
