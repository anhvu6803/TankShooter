using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;
    [SerializeField] private ServerSingleton serverPrefab;
    [SerializeField] private NetworkObject playerPrefab;

    private const string GameScene = "Game";
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
            Application.targetFrameRate = 60;
            appData = new ApplicationData();
            ServerSingleton serverSingleton = Instantiate(serverPrefab);
            StartCoroutine(LoadGameSceneAsync(serverSingleton));
        }
        else
        {
            HostSingleton hostInstance = Instantiate(hostPrefab);
            hostInstance.CreatHost(playerPrefab);

            ClientSingleton clientInstance = Instantiate(clientPrefab);
            bool authenticated = await clientInstance.CreatClient();

            if (authenticated)
            {
                clientInstance.GameManager.GoToMenu();
            }
        }
    }
    private IEnumerator LoadGameSceneAsync(ServerSingleton serverSingleton)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GameScene);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        Task createServerTask = serverSingleton.CreatServer(playerPrefab);
        yield return new WaitUntil(() =>  createServerTask.IsCompleted);

        Task startServerTask =  serverSingleton.GameManager.StartGameServerAsync();
        yield return new WaitUntil(() => startServerTask.IsCompleted);
    }
}
