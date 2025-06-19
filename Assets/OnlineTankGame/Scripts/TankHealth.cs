using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class TankHealth : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int currentHealth = 100;
    public int maxHealth = 100;

    public GameObject explosionEffect;
    public GameOverUI gameOverUI;

    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public Image healthBarFill; // ← Can barı için Image (Fill Amount kullanan)

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    public override void OnStartClient()
    {
        base.OnStartClient();
        gameOverUI = FindObjectOfType<GameOverUI>();
        UpdateHealthBar();
        nameText.text = playerName;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        GameManager.Instance.RegisterPlayer(this);

        // Server'da rastgele oyuncu ismi atama
        playerName = $"Player{Random.Range(100, 999)}";
    }

    [Server]
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            RpcDie();
            GameManager.Instance.PlayerDied(this);
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (isLocalPlayer)
        {
            gameObject.SetActive(false);
        }
    }

    void OnNameChanged(string oldName, string newName)
    {
        if (nameText != null)
        {
            nameText.text = newName;
        }
    }

    void OnHealthChanged(int oldHealth, int newHealth)
    {
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }


    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OutOfBounds"))
        {
            TakeDamage(100); // Direkt öldürmek için 100 verebilirsin
        }
    }


}


