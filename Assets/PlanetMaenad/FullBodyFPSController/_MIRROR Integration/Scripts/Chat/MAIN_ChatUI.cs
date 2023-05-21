using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;


//namespace Mirror.Examples.Port
//{

//}


namespace PlanetMaenad.FPS
{
    public class MAIN_ChatUI : NetworkBehaviour
    {
        public GameObject MainWindowPanel;
        [Space(10)]


        [Header("UI Elements")]
        public Text chatHistory;
        public Scrollbar scrollbar;
        public InputField chatMessage;
        public Button sendButton;
        [Space(10)]

        public Button showChatButton;
        public Button showChangeSceneButton;




        // This is only set on client to the name of the local player
        public static string localPlayerName;
        // Server-only cross-reference of connections to player names
        internal static readonly Dictionary<NetworkConnectionToClient, string> connNames = new Dictionary<NetworkConnectionToClient, string>();



        public override void OnStartServer()
        {
            connNames.Clear();
        }
        public override void OnStartClient()
        {
            chatHistory.text = "";
        }




        [Command(requiresAuthority = false)]
        void CmdSend(string message, NetworkConnectionToClient sender = null)
        {
            if (!connNames.ContainsKey(sender))
                connNames.Add(sender, sender.identity.GetComponent<Mirror_MasterPlayerController>().playerName);

            if (!string.IsNullOrWhiteSpace(message))
                RpcReceive(connNames[sender], message.Trim());
        }

        [ClientRpc]
        void RpcReceive(string playerName, string message)
        {
            string prettyMessage = playerName == localPlayerName ?
                $"<color=#FFCA00>{playerName}:</color> {message}" :
                $"<color=#93C5DE>{playerName}:</color> {message}";
            AppendMessage(prettyMessage);
        }
        [ClientRpc]
        public void RpcReceiveGlobalMessage(string message)
        {
            string prettyMessage = message;
            AppendMessage(prettyMessage);
        }


        void AppendMessage(string message)
        {
            StartCoroutine(AppendAndScroll(message));
        }
        IEnumerator AppendAndScroll(string message)
        {
            chatHistory.text += message + "\n";

            // it takes 2 frames for the UI to update ?!?!
            yield return null;
            yield return null;

            // slam the scrollbar down
            scrollbar.value = 0;
        }



        // Called by UI element ExitButton.OnClick
        public void ExitButtonOnClick()
        {
            // StopHost calls both StopClient and StopServer
            // StopServer does nothing on remote clients
            NetworkManager.singleton.StopHost();
        }
        // Called by UI element MessageField.OnValueChanged
        public void ToggleButton(string input)
        {
            sendButton.interactable = !string.IsNullOrWhiteSpace(input);
        }
        // Called by UI element MessageField.OnEndEdit
        public void OnEndEdit(string input)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetButtonDown("Submit"))
                SendMessage();
        }
        // Called by OnEndEdit above and UI element SendButton.OnClick
        public void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(chatMessage.text))
            {
                CmdSend(chatMessage.text.Trim());
                chatMessage.text = string.Empty;
                chatMessage.ActivateInputField();
            }
        }

    }
}
