using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Audio
{
    public class Music : MonoBehaviourSingletonPersistent<Music>
    {
        public AudioClip menuMusic, kitchenMusic, marketMusic;
        private AudioSource audioSource;

        void Start(){
            audioSource = this.GetComponent<AudioSource>();
        }

        void Update(){
            SwapMusic();
        }

        public void SwapMusic(){
            
        }
    }
}

