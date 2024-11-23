using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

namespace Game.Audio
{
    public class Music : MonoBehaviourSingletonPersistent<Music>
    {
        public AudioMixerGroup audioMixerGroup;
        public AudioClip menuMusic, marketMusic, mainMusic;
        public AudioSource audioSource01, audioSource02;

        void Start(){
            audioSource01.outputAudioMixerGroup = audioMixerGroup;
            audioSource02.outputAudioMixerGroup = audioMixerGroup;
            audioSource01.loop = true;
            audioSource02.loop = true;
            SceneManager.sceneLoaded += SwapMusic;
            SwapMusic(SceneManager.GetActiveScene());
        }


        public void SwapMusic(Scene scene){
            StopAllCoroutines();
            StartCoroutine(FadeTrack(scene));
            /*if(scene.name == "Menu"){
                audioSource01.clip = menuMusic;
                audioSource02.Stop();
                audioSource01.Play();
            }
            if(scene.name == "Game"){
                audioSource02.clip = mainMusic;
                audioSource01.Stop();
                audioSource02.Play();
            }*/

        }

        public void SwapMusic(Scene scene, LoadSceneMode load){
            SwapMusic(scene);
        }

        private IEnumerator FadeTrack(Scene scene)
        {
            float timeToFade = 1.0f;
            float timeElapsed = 0;

            if (scene.name == "Menu")
            {
                audioSource01.clip = menuMusic;
                audioSource01.Play();

                while (timeElapsed < timeToFade)
                {
                    audioSource01.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                    audioSource02.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                       
                audioSource02.Stop();
            }
            else
            {
                audioSource02.clip = mainMusic;
                audioSource02.Play();

                while (timeElapsed < timeToFade)
                {
                    audioSource01.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
                    audioSource02.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }
                
                audioSource01.Stop();
            }
        }
    }
}

