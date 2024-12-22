using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;
    [SerializeField] private ServerSingleton serverPrefab;

    private ApplicationData appData;
    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }
    private async Task LaunchInMode(bool isDedecatedServer)
    {
        if (isDedecatedServer)
        {
            appData = new ApplicationData();
            ServerSingleton serverSingleton = Instantiate(serverPrefab);
            await serverSingleton.CreatServer();
            await serverSingleton.GameManager.StartGameServerAsync();
        }
        else
        {
            HostSingleton hostInstance = Instantiate(hostPrefab);
            hostInstance.CreatHost();

            ClientSingleton clientInstance = Instantiate(clientPrefab);
            bool authenticated = await clientInstance.CreatClient();

            if (authenticated)
            {
                clientInstance.GameManager.GoToMenu();
            }
        }
    }
}
