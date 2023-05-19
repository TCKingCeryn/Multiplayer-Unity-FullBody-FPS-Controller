using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mirror;


namespace PlanetMaenad.FPS
{
    public class MAIN_NetworkManager : NetworkManager
    {

        public UnityEvent OnStartServerEvent;
        public UnityEvent OnStopServerEvent;
        [Space(10)]

        public UnityEvent OnStartClientEvent;
        public UnityEvent OnStopClientEvent;



        public override void OnStartServer()
        {
            OnStartServerEvent.Invoke();
        }
        public override void OnStartClient()
        {
            OnStartClientEvent.Invoke();
        }



        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            OnStopServerEvent.Invoke();

            // remove player name from the HashSet
            if (conn.authenticationData != null)
                Mirror_MasterPlayerController.playerNames.Remove((string)conn.authenticationData);

            // remove connection from Dictionary of conn > names
            MAIN_ChatUI.connNames.Remove(conn);

            base.OnServerDisconnect(conn);
        }
        public override void OnClientDisconnect()
        {
            OnStopClientEvent.Invoke();

            base.OnClientDisconnect();
            MAIN_LoginManager.instance.gameObject.SetActive(true);
        }

    }
}
