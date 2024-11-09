using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
public class SceneLoader : Singleton<SceneLoader>
{
    //[SerializeField] Slider slider;
    bool isLoading = false;
    /// <summary>
    /// Cleans after every load
    /// </summary>
    Action sceneLoaded;
    private void Awake()
    {
        UnitySceneManager.sceneLoaded += UnitySceneManager_sceneLoaded;
    }

    private void UnitySceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.Log("a");
        sceneLoaded?.Invoke();
        sceneLoaded = null;
    }

    public void ServerLoadLevel(Scenes scene, Action doAfterLoad = null)
    {
        sceneLoaded += doAfterLoad;
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }
    public void LoadLevel(Scenes scene, Action doAfterLoad = null) => LoadLevel(UnitySceneManager.GetSceneByBuildIndex((int)scene).name, doAfterLoad);
    public void LoadLevel(int sceneIndex, Action doAfterLoad = null) => LoadLevel(UnitySceneManager.GetSceneByBuildIndex(sceneIndex).name, doAfterLoad);
    public void LoadLevel(string sceneName, Action doAfterLoad = null)
    {
        if (isLoading) return;
        LoadAsync(sceneName, doAfterLoad);
    }
    public void LoadAdditiveScene(Scenes scene) => LoadAdditiveScene(UnitySceneManager.GetSceneByBuildIndex((int)scene).name);
    public void LoadAdditiveScene(int sceneIndex) => LoadAdditiveScene(UnitySceneManager.GetSceneByBuildIndex(sceneIndex).name);
    public void LoadAdditiveScene(string sceneName)
    {
        UnitySceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void UnloadAdditiveScenes()
    {
        if (UnitySceneManager.sceneCount > 1)
        {
            for (int i = 1; i < UnitySceneManager.sceneCount; i++)
            {
                if (UnitySceneManager.GetSceneAt(i).isSubScene)
                    UnitySceneManager.UnloadSceneAsync(UnitySceneManager.GetSceneAt(i));
            }
        }
    }

    public void RestartLevel()
    {
        if (isLoading) return;
        isLoading = true;
        LoadAsync(UnitySceneManager.GetActiveScene().name);
    }

    public void CloseGame()
    {
        Debug.Log("bye :D");
        Application.Quit();
    }

    //Shows the async loading process via the referenced slide, then load the scene
    void LoadAsync(string sceneName, Action doAfterLoad = null)
    {
        isLoading = true;
        Time.timeScale = 1;
        //DataCarrier.HoldData((Scene arg0, LoadSceneMode arg1) =>
        //{
        //    if ((string.IsNullOrEmpty(NaninovelManager.Instance?.StartStoryName) || (NaninovelManager.Instance?.IsRunning ?? true)) && 
        //    UnitySceneManager.GetActiveScene().name != NaninovelManager.NANINOVEL_SCENE 
        //    && Fight.FightGameManager.Instance == null && Fight.FightLevelManager.Instance == null)
        //        Transition.Instance.StartTransition(Transition.TransitionType.StrokesReverse);
        //});
        //Transition.Instance.StartTransition(() => {
        //    Engine.Destroy();
        //    }, Transition.TransitionType.Strokes, false);
        UnloadAdditiveScenes();
        UnitySceneManager.LoadSceneAsync(sceneName);
        sceneLoaded += doAfterLoad;
    }

    // Keep in the same order as the build scenes order
    public enum Scenes
    {
        Menu,
        Game,
    }
}
