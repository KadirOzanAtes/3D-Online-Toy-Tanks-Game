using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public float speed = 10f;
    public float lifeTime = 5f;
    public int damage = 25;

    public GameObject impactEffectPrefab; // <-- efekt prefabı

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), lifeTime);
    }

    void Update()
    {
        if (isServer)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        // Eğer oyuncuya çarptıysa hasar ver
        if (other.CompareTag("Player"))
        {
            TankHealth health = other.GetComponent<TankHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        // Çarpma efektini oluştur
        RpcPlayImpactEffect(transform.position);

        // Mermiyi yok et
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    void RpcPlayImpactEffect(Vector3 position)
    {
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, position, transform.rotation);
        }
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}



