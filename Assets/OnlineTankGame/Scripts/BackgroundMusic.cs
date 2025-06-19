using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // Başka varsa kendini sil
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Sahne geçişinde silinmesin

        GetComponent<AudioSource>().Play();
    }
}

