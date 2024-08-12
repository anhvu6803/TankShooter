using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;
    public ClientGameManager GameManager { get; private set; }
    public static ClientSingleton Instances
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            instance = FindObjectOfType<ClientSingleton>();
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
    public async Task<bool> CreatClient()
    {
        GameManager = new ClientGameManager();

        return await GameManager.InitAsync();
    }
}
