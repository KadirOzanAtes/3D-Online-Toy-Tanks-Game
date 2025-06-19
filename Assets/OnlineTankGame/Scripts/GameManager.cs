using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private List<TankHealth> alivePlayers = new List<TankHealth>();

    public GameOverUI gameOverUIPrefab;

    void Awake()
    {
        Instance = this;
    }

    [ServerCallback]
    public void RegisterPlayer(TankHealth player)
    {
        if (!alivePlayers.Contains(player))
            alivePlayers.Add(player);
    }

    [Server]
    public void PlayerDied(TankHealth player)
    {
        if (alivePlayers.Contains(player))
            alivePlayers.Remove(player);

        if (alivePlayers.Count == 1)
        {
            RpcShowWinner(alivePlayers[0].playerName);
            Debug.Log("Kazanan: " + alivePlayers[0].playerName);
        }
    }

    [ClientRpc]
    void RpcShowWinner(string winnerName)
    {
        var ui = FindObjectOfType<GameOverUI>();
        if (ui != null)
        {
            ui.ShowWinner(winnerName);

            // Sadece host için restart butonunu aktif et
            if (NetworkServer.active && NetworkClient.active)
            {
                ui.ShowRestartButton(); // Bu metodu GameOverUI içinde yazacağız
            }

            else
            {
                Debug.LogWarning("GameOverUI bulunamadı!");
            }
        }
    }

    // Host'tan çağrılır
    [Server]
    public void RestartGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            // Tüm client'leri kapat
            NetworkManager.singleton.StopHost(); 
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }

        // Sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}


