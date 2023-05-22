using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace PlanetMaenad.FPS
{
    public class MAIN_LoginManager : MonoBehaviour
    {

        public MAIN_NetworkManager NetworkManager;
        public MAIN_ChatAuthenticator ChatAuthenticator;
        [Space(10)]


        public int PlayerPrefabIndex;
        public GameObject[] PlayerPreviewObjects;
        [Space(10)]

        public GameObject LoginUI;
        public Button[] MenuButtons;



        public static MAIN_LoginManager instance;



        void Awake()
        {
            instance = this;
        }


        public void SetPlayerPrefab(int Index)
        {
            NetworkManager.PlayerPrefabIndex = Index;

            for (int i = 0; i < PlayerPreviewObjects.Length; i++)  //The "iBall" for-loop Goes through all of the Array.
            {
                PlayerPreviewObjects[i].SetActive(false);
            }

            PlayerPreviewObjects[Index].SetActive(true);
        }
        public void SetNewName(string PlayerName)
        {
            ChatAuthenticator.SetPlayerName(PlayerName);
            ToggleButtons(true);

            //PlayerPrefs.SetString("PlayerName", PlayerName);
        }
        public void SetHostName(string HostName)
        {
            NetworkManager.SetHostname(HostName);
        }



        public void StartHost()
        {
            if (!NetworkServer.active)
            {
                NetworkManager.StartHost();

                //LoginUI.SetActive(false);
            }
        }
        public void StartClient()
        {
            if (!NetworkClient.active)// && NetworkManager.isNetworkActive)
            {
                NetworkManager.StartClient();

                //LoginUI.SetActive(false);
            }
        }

        public void QuitApplication()
        {
            Application.Quit();
        }



        public void ToggleButtons(string username)
        {
            ToggleButtons(!string.IsNullOrWhiteSpace(username));
        }
        public void ToggleButtons(bool Bool)
        {
            for (int i = 0; i < MenuButtons.Length; i++)  //The "iBall" for-loop Goes through all of the Array.
            {
                MenuButtons[i].interactable = Bool;
            }
        }
    }
}
