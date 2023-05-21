using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Mirror_ChangeServerScene : NetworkBehaviour
{

    public string destinationScene;
    public Vector3 startPosition;




    public void ChangeDestinationScene(string manualScene)
    {
        destinationScene = manualScene;
    }


    public void StopServer()
    {
        NetworkManager.singleton.StopHost();
    }


    public void ChangeServerScene()
    {
        NetworkManager.singleton.ServerChangeScene(destinationScene);
    }
    public void ChangeServerScene(string manualScene)
    {
        NetworkManager.singleton.ServerChangeScene(manualScene);
    }

}
