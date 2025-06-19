using UnityEngine;
using UnityEngine.UI;
using System.Collections;



[RequireComponent(typeof(CharacterController))]
public class MouseControllerYedek : MonoBehaviour
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
        if (!isCaptured)
        {
            Move();
            HandleCoyoteTime();
        }
    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(horizontal, 0f, vertical).normalized;

        if (move.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + playerCamera.transform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }

        animator.SetFloat("Speed", move.magnitude);

        if (coyoteTimeActive && Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
            isJumping = true;
            animator.SetBool("IsJumping", true);
            coyoteTimer = 0f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded && isJumping)
        {
            isJumping = false;
            animator.SetBool("IsJumping", false);
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

    public void GetCaught(Transform mouth)
    {
        if (isCaptured) return;

        isCaptured = true;
        controller.enabled = false;

        foreach (var rend in renderers)
            rend.enabled = false;

        DropCheese();

        transform.SetParent(mouth);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

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

        Respawn();
    }

    void Respawn()
    {
        transform.SetParent(null);
        transform.position = spawnPoint.position;
        controller.enabled = true;

        foreach (var rend in renderers)
            rend.enabled = true;

        isCaptured = false;

        if (respawnUI != null)
            respawnUI.SetActive(false);

        // Kamera geri geçişi 👇
        if (deathCam != null) deathCam.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
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




}

