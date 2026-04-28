using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement parameters")]
    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 moveDirection;
    private Vector2 moveInput;
    private float verticalInput;
    private float horizontalInput;
    private bool attackLocked = false;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravityMultiplier = -1f;
    [HideInInspector] public bool isGrounded;
    private Collider platformCollider; // Collider for platforms player stands next to
    private bool canJump = true;
    private bool canHop = false;
    
    private Transform cameraPOV;
    private Rigidbody playerRB;
    private Animator animator;
    private PlayerHunger playerHunger;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
        playerHunger = GetComponent<PlayerHunger>();
        animator = GetComponentInChildren<Animator>();
        cameraPOV = Camera.main.transform;
    }

    private void Start()
    {
        InputManager.instance.OnJump += Jump;
        animator.applyRootMotion = false;
    }

    private void OnDestroy()
    {
        InputManager.instance.OnJump -= Jump;
    }

    private void Update()
    {
        moveInput = InputManager.instance.HandleMovementInput();
        SetMovementInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (attackLocked) return;
        HandleMovement();
        HandleRotation();
    }

    private void SetMovementInput()
    {
        verticalInput = moveInput.y;
        horizontalInput = moveInput.x;
    }

    private void HandleMovement()
    {
        Vector3 cameraForward = cameraPOV.forward;
        Vector3 cameraRight = cameraPOV.right;
        cameraForward.y = 0;
        cameraRight.y = 0;

        moveDirection = cameraForward * verticalInput;
        moveDirection += cameraRight * horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (InputManager.instance.isSprinting && playerHunger.currentHunger > 0) {moveDirection *= sprintSpeed;}
        else {moveDirection *= movementSpeed;}

        Vector3 moveVelocity = playerRB.linearVelocity;
        moveVelocity.x = moveDirection.x;
        moveVelocity.z = moveDirection.z;
        playerRB.linearVelocity = moveVelocity;
    }

    private void HandleRotation()
    {
        Vector3 targetDirection;
        if (moveInput == Vector2.zero) return;

        targetDirection = cameraPOV.forward * verticalInput;
        targetDirection += cameraPOV.right * horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;
    
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }

    public void LockMove(bool value)
    {
        attackLocked = value;
        canJump = !value;
        if (attackLocked) StopMovement();
    }

    public IEnumerator GetKnockedBack(float forceMagnitude, Vector3 direction)
    {
        direction.y += 0.3f;
        direction.Normalize();
        LockMove(true);
        playerRB.AddForce(direction * forceMagnitude, ForceMode.Impulse);

        yield return new WaitForSeconds(0.2f);
        LockMove(false);
    }

    public Quaternion GetCameraForward()
    {
        Vector3 cameraForward = cameraPOV.forward;
        cameraForward.y = 0f;

        if (cameraForward == Vector3.zero) return transform.rotation;

        return Quaternion.LookRotation(cameraForward);
    }

    private void StopMovement()
    {
        Vector3 velocity = playerRB.linearVelocity;
        velocity.x = 0f;
        velocity.z = 0f;

        playerRB.linearVelocity = velocity;
    }

    public IEnumerator WalkTo(Vector3 targetPosition, float duration)
    {
        LockMove(true);

        float timer = 0f;

        while (timer < duration)
        {
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0;

            if (direction.magnitude > 0.1f)
            {
                direction.Normalize();

                Vector3 velocity = playerRB.linearVelocity;
                velocity.x = direction.x * movementSpeed;
                velocity.z = direction.z * movementSpeed;
                playerRB.linearVelocity = velocity;

                // Rotate toward toy
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Vector3 stopVel = playerRB.linearVelocity;
        stopVel.x = 0;
        stopVel.z = 0;
        playerRB.linearVelocity = stopVel;

        LockMove(false);
    }

    // Jump functions
    private void Jump(object sender, EventArgs e)
    {
        //Debug.Log("Grounded: " + isGrounded);
        //Debug.Log("CanJump: " + canJump);
        //Debug.Log("CanHop: " + canHop);
        //Debug.Log("Platform Collider: "+ platformCollider);
        if (!isGrounded) return;

        if (canHop)
        {
            StartCoroutine(HopUpCoroutine());
        }
        else if (canJump)
        {
            StartCoroutine(JumpCoroutine());
        }
    }

    private IEnumerator JumpCoroutine()
    {
        float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
        canJump = false;

        Vector3 playerVelocity = playerRB.linearVelocity;
        playerVelocity.y = jumpVelocity;
        playerRB.linearVelocity = playerVelocity;
        StartCoroutine(FallingCoroutine());
        yield return new WaitWhile(() => isGrounded == false);
        canJump = true;
    }
    
    private IEnumerator HopUpCoroutine()
    {
        float colliderHeight = platformCollider.bounds.max.y;
        Debug.Log("Collider height: " + colliderHeight);
        float playerFeet = gameObject.GetComponent<CapsuleCollider>().bounds.min.y;
        float buffer = 0.7f;

        if ((colliderHeight - playerFeet) <= 0) yield break;

        float jumpVelocity = Mathf.Sqrt((colliderHeight - playerFeet + buffer) * -2f * Physics.gravity.y);

        LockMove(true);

        Vector3 playerVelocity = playerRB.linearVelocity;
        playerVelocity.y = jumpVelocity;
        playerRB.linearVelocity = playerVelocity;
        
        StartCoroutine(FallForwardCoroutine());
    }

    private IEnumerator FallingCoroutine()
    {
        yield return new WaitUntil(() => playerRB.linearVelocity.y == 0);

        while (!isGrounded)
        {
            playerRB.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1) * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator FallForwardCoroutine()
    {
        Vector3 forwardDirection = platformCollider.transform.position - transform.position;
        forwardDirection.y = 0f;
        float forwardVelocity = 3f;
        float currentYVelocity = playerRB.linearVelocity.y;

        Debug.Log(forwardDirection);

        yield return new WaitWhile(() => playerRB.linearVelocity.y > currentYVelocity/2);
        Debug.Log("falling forward");
        while (!isGrounded)
        {
            playerRB.linearVelocity += forwardDirection * forwardVelocity * Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }
        //Debug.Log("Ending foward motion");
        LockMove(false);
    }

    // Check if jumping or hopping
    void OnTriggerEnter(Collider other)
    {
        BoxCollider[] colliders = other.GetComponentsInParent<BoxCollider>();
        if (other.CompareTag("Hoppable"))
        {
            canJump = false;
            canHop = true;
            foreach (BoxCollider col in colliders)
            {
                if (!col.isTrigger)
                {
                    platformCollider = col;
                    break;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hoppable"))
        {
            canJump = true;
            canHop = false;
            platformCollider = null;
        }
    }

    private void UpdateAnimation()
    {
        float multiplier = (InputManager.instance.isSprinting && playerHunger.currentHunger > 0) ? 2f : 1f; //Change values based on walking and sprinting
        float dampTime = 0.3f;

        Vector2 input = moveInput.normalized * Mathf.Clamp01(moveInput.magnitude) * multiplier;

        animator.SetFloat("horizontal", input.x, dampTime, Time.deltaTime);
        animator.SetFloat("vertical", input.y, dampTime, Time.deltaTime);

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("jumpVelocity", playerRB.linearVelocity.y);
    }
}
