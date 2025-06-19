using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        MouseController mouse = other.GetComponent<MouseController>();

        if (mouse != null && mouse.HasCheese())
        {
            GameObject cheese = mouse.GetCarriedCheese(); // Taşınan peynire ulaş
            mouse.ForceDropCheese(); // Ağzından düşür (referansı sıfırlar)
            
            if (cheese != null)
            {
                Destroy(cheese); // Kalıcı olarak yok et
            }

            ScoreManager.Instance.AddScore(1); // Skoru artır
        }
    }
}



