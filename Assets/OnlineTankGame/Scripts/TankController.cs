using UnityEngine;
using Mirror;

public class TankController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 100f;

    public Transform turret;
    public GameObject bulletPrefab;
    public Transform firePoint;

    private float fireCooldown = 1f;
    private float lastFireTime = -999f;

    void Update()
    {
        if (!isLocalPlayer) return;  // Sadece local player kendi girişlerini işlesin

        HandleMovement();
        HandleTurret();
        HandleFire();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
    }

    void HandleTurret()
    {
        if (Input.GetKey(KeyCode.O))
            turret.Rotate(Vector3.forward * turnSpeed * Time.deltaTime);
        else if (Input.GetKey(KeyCode.P))
            turret.Rotate(-Vector3.forward * turnSpeed * Time.deltaTime);

        // Turret dönüşünü -90 ile 90 derece arasında sınırla
        Vector3 rot = turret.localEulerAngles;
        if (rot.z > 180) rot.z -= 360;
        rot.z = Mathf.Clamp(rot.z, -90f, 90f);
        turret.localEulerAngles = new Vector3(rot.x, rot.y, rot.z);
    }

    void HandleFire()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastFireTime >= fireCooldown)
        {
            lastFireTime = Time.time;
            CmdFire();
        }
    }

    [Command]
    void CmdFire()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = firePoint.forward * 20f;

        NetworkServer.Spawn(bullet);
        Destroy(bullet, 5f); // Mermiyi 5 saniye sonra yok et
    }
}

