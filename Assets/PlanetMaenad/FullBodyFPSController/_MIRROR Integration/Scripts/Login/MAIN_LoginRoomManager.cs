using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace PlanetMaenad.FPS
{
    public class MAIN_LoginRoomManager : MonoBehaviour
    {

        public MAIN_NetworkRoomManager NetworkRoomManager;
        public MAIN_ChatAuthenticator ChatAuthenticator;
        [Space(10)]

        public KeyCode ToggleCursorButton = KeyCode.X;
        public Texture2D CursorSprite;
        public bool ShowCursor = true;
        [Space(10)]

        public GameObject RoomUI;
        public GameObject LoginUI;
        public Button[] MenuButtons;



        public static MAIN_LoginRoomManager instance;

        internal WaitForSeconds TinyDelay = new WaitForSeconds(0.1f);
        internal WaitForSeconds SmallDelay = new WaitForSeconds(0.2f);
        internal WaitForSeconds MedDelay = new WaitForSeconds(0.5f);

        internal WaitForEndOfFrame WaitForEndOfFrameDelay;


        void Awake()
        {
            instance = this;
        }
        void Start()
        {
            if (ShowCursor)
            {
                if (CursorSprite) Cursor.SetCursor(CursorSprite, Vector2.zero, CursorMode.Auto);

                //ToggleCursor();
            }
        }
        void Update()
        {
            if (Input.GetKeyDown(ToggleCursorButton))
            {
                ToggleCursor();
            }
        }


        public void ToggleCursor()
        {
            if (Cursor.visible == false)
            {
                StartCoroutine(ToggleCursorDelay(true));
            }
            if (Cursor.visible == true)
            {
                StartCoroutine(ToggleCursorDelay(false));
            }
        }
        IEnumerator ToggleCursorDelay(bool Bool)
        {
            yield return WaitForEndOfFrameDelay;

            if (Bool == true)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            if (Bool == false)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public void SetNewName(string PlayerName)
        {
            ChatAuthenticator.SetPlayerName(PlayerName);
            ToggleButtons(true);

            //PlayerPrefs.SetString("PlayerName", PlayerName);
        }
        public void SetHostName(string HostName)
        {
            NetworkRoomManager.SetHostname(HostName);
        }
        public void StartHost()
        {
            if (!NetworkServer.active)
            {
                NetworkRoomManager.StartHost();

                LoginUI.SetActive(false);
                RoomUI.SetActive(true);
            }
        }
        public void StartClient()
        {
            if (!NetworkClient.active)
            {
                NetworkRoomManager.StartClient();

                LoginUI.SetActive(false);
                RoomUI.SetActive(true);
            }
        }


        public void QuitRoom()
        {
            if (NetworkServer.active)
            {
                NetworkRoomManager.StopHost();
                LoginUI.SetActive(true);
                RoomUI.SetActive(false);
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
