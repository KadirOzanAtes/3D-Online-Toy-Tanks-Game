using UnityEngine;
using Mirror;

public class NetworkManagerTankGame : NetworkManager
{
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Spawn noktalarını otomatik al (gerekirse)
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
            spawnPoints = new Transform[spawnObjects.Length];
            for (int i = 0; i < spawnObjects.Length; i++)
            {
                spawnPoints[i] = spawnObjects[i].transform;
            }

            if (spawnPoints.Length == 0)
            {
                Debug.LogError("Spawn point bulunamadı! Sahneye 'SpawnPoint' tagli nesneler ekleyin.");
                return;
            }
        }

        int index = numPlayers % spawnPoints.Length;
        Transform startPos = spawnPoints[index];

        GameObject player = Instantiate(playerPrefab, startPos.position, startPos.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}



