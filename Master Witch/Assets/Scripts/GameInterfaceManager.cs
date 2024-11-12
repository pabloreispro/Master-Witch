using Network;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class GameInterfaceManager : SingletonNetwork<GameInterfaceManager>
    {
        [Header("Final HUD")]
        public GameObject[] playerUI;
        public TextMeshProUGUI[] playerFinalScore;
        [SerializeField] TextMeshProUGUI[] textScore;
        public Toggle[] playerFinalCheck;
        public GameObject finalPanel;
        public GameObject ResultPanel;
        [Header("Game HUD")]
        [SerializeField] GameObject gameMenuHUD;
        public GameObject recipeSteps, dialogueBox, clock, horizontalGroupPrefab, imagePrefab;
        public Sprite plusSprite, equalsSprite, arrowSprite, benchOven, benchBoard, benchStove;
        public TextMeshProUGUI recipeName;

        public void OpenGameMenu(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (obj.performed)
                gameMenuHUD.SetActive(!gameMenuHUD.activeSelf);
        }
        public void OnGameStartedClient()
        {
            EliminationPlayer.Instance.AddScoresPlayers();
            GameManager.Instance.numberPlayer = PlayerNetworkManager.Instance.PlayersData.Length;
        }
        public void DisconnectFromServer()
        {
            LobbyManager.Instance.DisconnectFromServer();
        }
        public void UpdatePlayerScore(int playerID, float score)
        {
            Debug.Log("Update " + playerID);
            string name = PlayerNetworkManager.Instance.PlayersData[playerID].PlayerName;
            playerFinalScore[playerID].text = name;
            textScore[playerID].text = score.ToString("00.00");

            //switch (playerID)
            //{

            //    case 0:
            //        playerFinalScore[0].text = name;
            //        textScore[0].text = score.ToString();
            //        break;
            //    case 1:
            //        playerFinalScore[1].text = name;
            //        textScore[1].text = score.ToString();
            //        break;
            //    case 2:
            //        playerFinalScore[2].text = name;
            //        textScore[2].text = score.ToString();
            //        break;
            //    case 3:
            //        playerFinalScore[3].text = name;
            //        textScore[3].text = score.ToString();
            //        break;
            //    default:
            //        break;
            //}
        }
        [ClientRpc]
        public void UpdateFinalRoundScreenClientRpc()
        {
            finalPanel.SetActive(true);
            for (int i = 0; i < GameManager.Instance.numberPlayer; i++)
            {
                playerUI[i].gameObject.SetActive(true);
                UpdatePlayerScore(i, EliminationPlayer.Instance.scoresPlayers.Values.Count <= i ? EliminationPlayer.Instance.scoresPlayers[i] : 0);
            }
            //foreach (var item in EliminationPlayer.Instance.scoresPlayers)
            //{
            //    UpdatePlayerScore(item.Key, item.Value);
            //}
        }

        public void UpdateToggle(int playerID, bool toggleValue)
        {
            playerFinalCheck[playerID].isOn = toggleValue;
        }

        [ClientRpc]
        public void UpdadeScreenFinalClientRpc()
        {
            EndRound.Instance.finalGame = true;
            UpdateFinalResult(EndRound.Instance.finishGame());
        }

        public void UpdateFinalResult(List<KeyValuePair<int, float>> orderPlayers)
        {
            ResultPanel.SetActive(true);

            for (int j = 0; j < GameManager.Instance.numberPlayer; j++)
            {
                playerUI[j].gameObject.SetActive(true);
                playerFinalCheck[j].isOn = false;
            }
            int i = 0;
            foreach (var item in orderPlayers)
            {
                playerFinalScore[i].text = PlayerNetworkManager.Instance.PlayersData[item.Key].PlayerName;
                textScore[i].text = item.Value.ToString();
                i++;
            }
        }

    }
}