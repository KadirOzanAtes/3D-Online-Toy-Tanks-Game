using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class GameOverUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI winnerText;
    public Button restartButton;

    void Start()
    {
        panel.SetActive(false);
        restartButton.gameObject.SetActive(false);
        restartButton.onClick.AddListener(OnRestartClicked);
    }

    public void ShowWinner(string winnerName)
    {
        panel.SetActive(true); // Paneli aktif et
        winnerText.text = $"Kazanan: {winnerName}";
        restartButton.gameObject.SetActive(NetworkServer.active); // sadece host görsün
    }

    public void ShowRestartButton()
    {
        restartButton.gameObject.SetActive(true);
    }

    void OnRestartClicked()
    {
        if (NetworkServer.active)
        {
            GameManager.Instance.RestartGame();
        }
    }
}



