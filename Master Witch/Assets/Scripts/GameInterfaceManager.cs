using Network;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI.Leaderboard;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

namespace Game.UI
{
    public class GameInterfaceManager : SingletonNetwork<GameInterfaceManager>
    {
        [Header("Final HUD")]
        [SerializeField] GameObject leaderboardPanel;
        [SerializeField] GameObject finalRoundPanel;
        [Header("Game HUD")]
        [SerializeField] GameObject gameMenuHUD;
        public GameObject recipeSteps, recipeStepsContent, dialogueBox, clock, horizontalGroupPrefab, imagePrefab;
        public Sprite plusSprite, equalsSprite, arrowSprite, benchOven, benchBoard, benchStove;
        public TextMeshProUGUI recipeName;

        public void OpenGameMenu(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (obj.performed)
                gameMenuHUD.SetActive(!gameMenuHUD.activeSelf);
        }
        public void DisconnectFromServer()
        {
            LobbyManager.Instance.DisconnectFromServer();
        }
        //public void UpdatePlayerScore(int playerID, float score)
        //{
        //    Debug.Log("Update " + playerID);
        //    string name = PlayerNetworkManager.Instance.PlayersData[playerID].PlayerName;
        //    playerFinalScore[playerID].text = name;
        //    textScore[playerID].text = score.ToString("00.00");
        //}
        [ClientRpc]
        public void EnableRoundScoresClientRpc()
        {
            leaderboardPanel.SetActive(true);
            //var leaderboard = LeaderboardManager.Instance.GetSortedPlayerScores();
            //for (int i = 0; i < leaderboard.Count; i++)
            //{
            //    playerUI[i].gameObject.SetActive(true);
            //    UpdatePlayerScore(leaderboard[i].Key, leaderboard[i].Value);
            //}
            //foreach (var item in EliminationPlayer.Instance.scoresPlayers)
            //{
            //    UpdatePlayerScore(item.Key, item.Value);
            //}
            LeaderboardManager.Instance.EnableRoundLeaderboard(GameManager.Instance.CurrentRound >= GameManager.Instance.TotalRounds);
        }

        //Server-Side
        public void ResetGameHUD()
        {
            ResetGameHUDClientRpc();
        }
        [ClientRpc]
        void ResetGameHUDClientRpc()
        {
            DisableRoundScores();
            //for (int i = 0; i < playerFinalCheck.Length; i++)
            //{
            //    UpdateToggle(i, false);
            //}
        }

        public void DisableRoundScores()
        {
            leaderboardPanel.SetActive(false);
            finalRoundPanel.SetActive(false);
        }


        //public void UpdateToggle(int playerID, bool toggleValue)
        //{
        //    playerFinalCheck[playerID].Toggle.isOn = toggleValue;
        //}

        //LIGA TELA FINAL, COM PONTUACAO FINAL E TODOS OS JOGADORES
        //[ClientRpc]
        //public void UpdadeScreenFinalClientRpc()
        //{
        //    EndRound.Instance.finalGame = true;
        //    UpdateFinalResult(EndRound.Instance.GetSortedPlayerScores());
        //}

        //public void UpdateFinalResult(List<KeyValuePair<int, float>> orderPlayers)
        //{
        //    finalRoundPanel.SetActive(true);

        //    var data = PlayerNetworkManager.Instance.PlayersData;
        //    for (int j = 0; j < data.Length; j++)
        //    {
        //        playerUI[j].gameObject.SetActive(true);
        //        playerFinalCheck[j].isOn = false;
        //    }
        //    int i = 0;
        //    foreach (var item in orderPlayers)
        //    {
        //        playerFinalScore[i].text = data[item.Key].PlayerName;
        //        textScore[i].text = item.Value.ToString();
        //        i++;
        //    }
        //}

    }
}