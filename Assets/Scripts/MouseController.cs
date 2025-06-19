using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Mirror;



[RequireComponent(typeof(CharacterController))]
public class MouseController : NetworkBehaviour
{
    [Header("Cheese Settings")]
    public Transform mouthPoint;
    private GameObject carriedCheese = null;

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 720f;
    public float jumpForce = 5f;

    [Header("References")]
    public Camera playerCamera;
    public Camera deathCam; // 👈 Yeni Eklendi
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = -9.81f;

    private bool isJumping = false;

    // Coyote Time
    private bool coyoteTimeActive = false;
    private float coyoteTime = 0.2f;
    private float coyoteTimer = 0f;

    // Respawn
    [Header("Respawn Settings")]
    public Transform spawnPoint;
    public Transform mouthHoldPointFromCat;
    public GameObject respawnUI;
    public Text respawnText;
    public float respawnTime = 5f;

    private bool isCaptured = false;
    private Renderer[] renderers;

    public bool HasCheese() => carriedCheese != null;

    public void DropCheese()
    {
        if (carriedCheese != null)
        {
            // CheeseRespawn varsa tekrar orijinal konuma gönder
            if (carriedCheese.TryGetComponent(out CheeseRespawn respawnScript))
            {
                respawnScript.Respawn();
            }

            carriedCheese = null;
        }
    }

    void Start()
    {

        if (!isLocalPlayer)
        {
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            if (respawnUI != null) respawnUI.SetActive(false);
            if (deathCam != null) deathCam.gameObject.SetActive(false);
            return;
        }


        controller = GetComponent<CharacterController>();
        renderers = GetComponentsInChildren<Renderer>();

        Cursor.lockState = CursorLockMode.Locked;

        if (respawnUI != null)
            respawnUI.SetActive(false);

        if (deathCam != null)
            deathCam.gameObject.SetActive(false); // Başta kapalı olmalı

    }

    void Update()
    {
        if (!isLocalPlayer || isCaptured) return;

        Move();
        HandleCoyoteTime();
    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            // Kamera yönüne göre hareket yönünü hesapla
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = (forward * vertical + right * horizontal).normalized;

            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }

        animator.SetFloat("Speed", inputDir.magnitude);

        // Zıplama
        if (coyoteTimeActive && Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
            isJumping = true;
            animator.SetBool("IsJumping", true);
            coyoteTimer = 0f;
        }

        // Yerçekimi
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Yere iniş kontrolü
        if (controller.isGrounded && isJumping)
        {
            isJumping = false;
            animator.SetBool("IsJumping", false);
        }

        // Karakterin yönünü kameranın yönüne sabitle (sadece yatay eksende)
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0f;

        if (camForward.sqrMagnitude > 0.01f)
        {
            transform.forward = camForward.normalized;
        }
    }

    void HandleCoyoteTime()
    {
        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        coyoteTimeActive = coyoteTimer > 0f;
    }

    void PickupCheese(GameObject cheese)
    {
        carriedCheese = cheese;
        cheese.transform.SetParent(mouthPoint);
        cheese.transform.localPosition = Vector3.zero;
        cheese.transform.localRotation = Quaternion.identity;

        if (cheese.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Cheese") && carriedCheese == null)
        {
            PickupCheese(hit.gameObject);
        }
    }

    public GameObject GetCarriedCheese()
    {
        return carriedCheese;
    }


    public void ForceDropCheese()
    {
        if (carriedCheese != null)
        {
            carriedCheese.transform.SetParent(null);
            carriedCheese = null;
        }
    }



    public override void OnStartAuthority()
    {
        playerCamera = Camera.main;

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);

            var follow = playerCamera.GetComponent<CameraFollow>();
            if (follow != null)
            {
                //follow.SetTarget(this.transform);
            }
        }

    }


    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Transform cameraRig = transform.Find("CameraRig");
        if (cameraRig != null)
        {
            Camera localCamera = cameraRig.GetComponentInChildren<Camera>(true);
            if (localCamera != null)
            {
                playerCamera = localCamera;
                localCamera.gameObject.SetActive(true);

                CameraFollow follow = localCamera.GetComponent<CameraFollow>();
                if (follow != null)
                {
                    follow.target = this.transform;
                }
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }





    public void CaughtByCat(GameObject mouth)
    {
        if (!isServer) return;

        RpcHandleCaught(mouth.transform.position, mouth.transform.rotation);
    }

    [ClientRpc]
    void RpcHandleCaught(Vector3 mouthPos, Quaternion mouthRot)
    {
        if (!isLocalPlayer) return;

        isCaptured = true;
        controller.enabled = false;

        foreach (var rend in renderers)
            rend.enabled = false;

        DropCheese();

        transform.position = mouthPos;
        transform.rotation = mouthRot;

        // Kamera geçişi 👇
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (deathCam != null) deathCam.gameObject.SetActive(true);

        StartCoroutine(RespawnCountdown());
    }



    IEnumerator RespawnCountdown()
    {
        if (respawnUI != null)
            respawnUI.SetActive(true);

        float timer = respawnTime;

        while (timer > 0)
        {
            if (respawnText != null)
                respawnText.text = "Yeniden Doğma: " + Mathf.Ceil(timer);
            timer -= Time.deltaTime;
            yield return null;
        }

        CmdRespawnRequest();
    }

    [Command]
    void CmdRespawnRequest()
    {
        RpcRespawn();
    }


    [ClientRpc]
    public void RpcRespawn()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnPoint not assigned!");
            return;
        }

        controller.enabled = false;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        controller.enabled = true;

        if (isLocalPlayer)
        {
            // DeathCamera kapat
            if (deathCam != null)
                deathCam.gameObject.SetActive(false);

            // Kendi CameraRig'imizi bulup içindeki kamerayı aktif et
            Transform cameraRig = transform.Find("CameraRig");
            if (cameraRig != null)
            {
                Camera localCamera = cameraRig.GetComponentInChildren<Camera>(true);
                if (localCamera != null)
                {
                    localCamera.gameObject.SetActive(true);
                
                    // 👉 BURASI EKLENECEK
                    playerCamera = localCamera;

                    // Eğer takip sistemi varsa yeniden ata
                    CameraFollow follow = localCamera.GetComponent<CameraFollow>();
                    if (follow != null)
                    {
                        follow.target = this.transform;
                    }
                }
                else
                {
                    Debug.LogWarning("CameraRig içinde kamera bulunamadı.");
                }
            }
            else
            {
                Debug.LogWarning("CameraRig bulunamadı!");
            }
        }
    }






    [ClientRpc]
    public void RpcDie()
    {
        if (isLocalPlayer)
        {
            // Ana kamerayı kapat
            if (Camera.main != null)
                Camera.main.gameObject.SetActive(false);

            GameObject deathCam = GameObject.Find("DeathCamera");
            if (deathCam != null)
                deathCam.SetActive(true);
            else
                Debug.LogWarning("DeathCamera not found!");
        }

        StartCoroutine(RespawnCountdown());
    }


}


