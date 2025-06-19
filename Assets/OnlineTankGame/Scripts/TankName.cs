using UnityEngine;
using TMPro;
using Mirror;

public class TankName : NetworkBehaviour
{
    public TextMeshProUGUI nameText;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    public override void OnStartLocalPlayer()
    {
        // Lokal oyuncu adını ayarla (isteğe bağlı olarak rastgele ya da sırayla da verebiliriz)
        string newName = $"Player{Random.Range(1, 1000)}";
        CmdSetName(newName);
    }

    [Command]
    void CmdSetName(string newName)
    {
        playerName = newName;
    }

    void OnNameChanged(string oldName, string newName)
    {
        if (nameText != null)
            nameText.text = newName;
    }
}


