using UnityEngine;

public class CheeseRespawn : MonoBehaviour
{
    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public Quaternion initialRotation;

    void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void Respawn()
    {
        transform.SetParent(null);
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;
    }
}

