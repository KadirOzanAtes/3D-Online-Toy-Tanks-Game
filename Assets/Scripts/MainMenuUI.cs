using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MainMenuUI : MonoBehaviour
{
    public string gameSceneName = "GameScene"; // Online sahnenin adı

    public void HostGame()
    {
        NetworkManager.singleton.StartHost(); // Host başlat
    }

    public void JoinGame()
    {
        NetworkManager.singleton.networkAddress = "192.168.1.107"; // Doğru IP olmalı
        NetworkManager.singleton.StartClient(); // Client başlat
    }
}
