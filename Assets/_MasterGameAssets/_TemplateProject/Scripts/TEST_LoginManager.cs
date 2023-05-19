using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class TEST_LoginManager : MonoBehaviour
{

    public TEST_NetworkManager NetworkManager;
    public TEST_ChatAuthenticator ChatAuthenticator;
    [Space(10)]

    public KeyCode ToggleCursorButton = KeyCode.X;
    public Texture2D CursorSprite;
    public bool ShowCursor = true;
    [Space(10)]


    public GameObject LoginUI;
    public Button[] MenuButtons;



    public static TEST_LoginManager instance;

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
        NetworkManager.SetHostname(HostName);
    }
    public void StartHost()
    {
        if (!NetworkServer.active)
        {
            NetworkManager.StartHost();

            LoginUI.SetActive(false);
        }
    }
    public void StartClient()
    {
        if (NetworkServer.active && !NetworkClient.active)
        {
            NetworkManager.StartClient();

            LoginUI.SetActive(false);
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
