using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Network;
using System.Linq;
using Unity.VisualScripting;
using UI;
using TMPro;
using Game.UI;
using Game.Audio;

namespace Game.SceneGame{
    public class SceneManager : SingletonNetwork<SceneManager>
    {
        public int TIMER_MARKET = 30;
        public const int TIMER_MAIN = 180;

        [SerializeField]
        private GameObject prefabMarket, prefabMain;
        
        public List<Transform> spawnPlayersMarket = new List<Transform>();
        public List<Transform> spawnPlayersMain = new List<Transform>();
        public List<Transform> spawnBasket = new List<Transform>();
        public List<Transform> spawnBasketMarket = new List<Transform>();
        public List<Storage> benchStorage = new List<Storage>();
        
        public NetworkVariable<bool> sceneMarket = new NetworkVariable<bool>();
        public NetworkVariable<bool> sceneMain = new NetworkVariable<bool>();
        public NetworkVariable<int> timeCount = new NetworkVariable<int>();
        public NetworkVariable<bool> isMovementAllowed = new NetworkVariable<bool>(false);
        
        

        public Transform clockHand; 
        public float maxTime = 0;  
        private float currentTime;

        public AudioClip marketClip, mainClip;

        
        [ServerRpc(RequireOwnership = false)]
        public void RepositionStorageMainSceneServerRpc(){
            for(int i =0; i< spawnBasket.Count; i++){
                var bench = benchStorage.ElementAt(i);
                bench.RepositionServerRpc(spawnBasket.ElementAt(i).position, spawnBasket.ElementAt(i).rotation);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RepositionStorageMarketSceneServerRpc(){
            for(int i =0; i< spawnBasketMarket.Count; i++){
                Debug.Log($"Repo bench {i}");
                var bench = benchStorage.ElementAt(i);
                bench.RepositionServerRpc(spawnBasketMarket.ElementAt(i).position, spawnBasketMarket.ElementAt(i).rotation);
                
            }
        }

        [ServerRpc (RequireOwnership = false)]
        public void RepositionPlayersMainSceneServerRpc()
        {
            RefillBenchesClientRpc();
            for (int i = 0; i < PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count; i++)
            {
                var player = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().ElementAt(i);
                var bench = benchStorage.ElementAt(i);
                //player.bench.ElementAt(i).ingredients.AddRange(player.ingredientsBasket);
                /*if(player.GetComponentInChildren<Tool>() != null){
                    var basket =  player.GetComponentInChildren<Tool>().gameObject;
                    basket.GetComponent<FollowTransform>().targetTransform = null;
                    basket.transform.position = new Vector3(spawnBasket.ElementAt(i).transform.position.x,spawnBasket.ElementAt(i).transform.position.y+20,spawnBasket.ElementAt(i).transform.position.z);
                    basket.transform.rotation = Quaternion.identity;
                    9
                    player.GetComponentInChildren<Tool>().GetComponentInChildren<NetworkObject>().TrySetParent(spawnBasket.ElementAt(i).transform);
                }*/
                bench.RepositionServerRpc(spawnBasket.ElementAt(i).position, spawnBasket.ElementAt(i).rotation);
                player.RepositionServerRpc(spawnPlayersMain.ElementAt(i).position); 
            }
            
        }

        [ServerRpc (RequireOwnership = false)]
        public void RepositionPlayersMarketSceneServerRpc()
        {
            for (int i = 0; i < PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count; i++)
            {
                var player = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().ElementAt(i);
                player.RepositionServerRpc(spawnPlayersMarket.ElementAt(i).position);
            }
        }


        [ClientRpc]
        void RefillBenchesClientRpc()
        {
            if (IsServer) return;
            for (int i = 0; i < PlayerNetworkManager.Instance.GetPlayer.Values.ToList().Count; i++)
            {
                var player = PlayerNetworkManager.Instance.GetPlayer.Values.ToList().ElementAt(i);
                //Debug.Log($"{player.NetworkObjectId}, {player.ingredientsBasket.Count}");
                //player.bench.ElementAt(i).ingredients.AddRange(player.ingredientsBasket);
                /*if(player.GetComponentInChildren<Tool>() != null){
                    player.GetComponentInChildren<Tool>().GetComponentInChildren<NetworkObject>().TrySetParent(spawnBasket.ElementAt(i).transform);
                }*/

            }
        }

        
        public void StartMarket()
        {
            timeCount.Value = TIMER_MARKET;
            maxTime = timeCount.Value;
            StartCoroutine(TimeCounter());
            GameManager.Instance.Reset();
        }

        
        public void StartMain()
        {
            timeCount.Value = TIMER_MAIN;
            maxTime = timeCount.Value;
            StartCoroutine(TimeCounter());
            GameManager.Instance.matchStartTime = Time.time;
            GameManager.Instance.Reset();
        }
            
        public void LoadMarket()
        {
            prefabMarket = GameObject.Find("MarketScene");
            spawnBasketMarket = new List<Transform>
            {
                GameObject.Find("SpawnStorage1").transform,
                GameObject.Find("SpawnStorage2").transform,
                GameObject.Find("SpawnStorage3").transform,
                GameObject.Find("SpawnStorage4").transform
            };
            spawnPlayersMarket = new List<Transform>
            {
                GameObject.Find("SpawnP1").transform,
                GameObject.Find("SpawnP2").transform,
                GameObject.Find("SpawnP3").transform,
                GameObject.Find("SpawnP4").transform
            };
            prefabMarket.SetActive(false);
            Debug.Log("Load");
        }
        public void ChangeScene(bool a, bool b)
        {
            ChangeSceneClientRpc(sceneMarket.Value = a, sceneMain.Value = b);
        }
        [ServerRpc(RequireOwnership = false)]
        public void ChangeSceneServerRpc(bool a, bool b)
        {
            ChangeSceneClientRpc(sceneMarket.Value = a, sceneMain.Value = b);
        }
        [ClientRpc]
        public void ChangeSceneClientRpc(bool a, bool b)
        {
            prefabMarket.SetActive(a);
            prefabMain.SetActive(b);
        }
        
        
        IEnumerator TimeCounter()
        {
            
            while(timeCount.Value > 0)
            {
                yield return new WaitForSeconds(1f);
                timeCount.Value -= 1;
                ClockTimeraServerRpc();
            }
            ControllerScenes();
        }
        
        public void ControllerScenes()
        {
            if(prefabMarket.activeSelf){
                StartCoroutine(TransitionController.Instance.TransitionMainScene());
            }
            else if(prefabMain.activeSelf){
                GameInterfaceManager.Instance.EnableRoundScoresClientRpc();
            }
        }
        [ServerRpc(RequireOwnership =false)]
        public void ClockTimeraServerRpc()
        {
        
            float angle = (Mathf.Max(0, timeCount.Value) / maxTime) * 360; 
            clockHand.eulerAngles = new Vector3(0, 0, angle);

            UpdateClockHandClientRpc(angle);
        }

        [ClientRpc]
        void UpdateClockHandClientRpc(float angle)
        {
            clockHand.eulerAngles = new Vector3(0, 0, angle);
        }
    }
}
