using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeText;
    public async void StartHost()
    {
        await HostSingleton.Instance.GameManager.StartHostAsync();
    }
    public async void StartClient()
    {
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeText.text);
    }
}
