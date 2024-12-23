using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;
    public HostGameManager GameManager { get; private set; }
    public static HostSingleton Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;  }
            instance = FindObjectOfType<HostSingleton>();
            if (instance == null )
          
            {
                Debug.LogError("No clientsingleton in the scene");
                return null;
            }
            return instance;
        }
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void CreatHost(NetworkObject playerPrefab)
    {
        GameManager = new HostGameManager(playerPrefab);
    }
    private void OnDestroy()
    {
        GameManager?.Dispose();
    }
}
