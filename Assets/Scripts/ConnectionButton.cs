using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConnectionButton : MonoBehaviour
{
    public void HostServer()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void JoinServer()
    {
        NetworkManager.Singleton.StartClient();
    }
}
