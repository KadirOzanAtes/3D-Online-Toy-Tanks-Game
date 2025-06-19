using UnityEngine;
using Mirror;

public class CameraFollow : NetworkBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -5f);
    public float smoothTime = 0.2f;
    public float rotationSpeed = 5f;

    private Vector3 currentVelocity;
    private float yaw = 0f;
    private float pitch = 10f;

    void Start()
    {
        if (target != null)
        {
            // Başlangıçta karakter yönüne göre ayarla
            yaw = target.eulerAngles.y;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * rotationSpeed;
        pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
        pitch = Mathf.Clamp(pitch, -30f, 60f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);


        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null && player.GetComponent<NetworkBehaviour>().isLocalPlayer)
            {
                target = player.transform;
            }
            else
            {
                return; // Hala yoksa çık
            }
        }



    }
}







