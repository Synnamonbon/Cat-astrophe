using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement parameters")]
    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 moveDirection;
    private Vector2 moveInput;
    private float verticalInput;
    private float horizontalInput;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravityMultiplier = -1f;
    [HideInInspector] public bool isGrounded;
    private bool canJump = true;
    
    private Transform cameraPOV;
    private Rigidbody playerRB;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
        cameraPOV = Camera.main.transform;
    }

    private void Start()
    {
        InputManager.instance.OnJump += Jump;
    }

    private void Update()
    {
        moveInput = InputManager.instance.HandleMovementInput();
        SetMovementInput();
    }

    private void FixedUpdate()
    {
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
        moveDirection *= movementSpeed;

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



    // Jump functions
    private void Jump(object sender, EventArgs e)
    {
        if (canJump && isGrounded)
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
        yield return new WaitWhile(() => isGrounded = false);
        canJump = true;
    }

    private IEnumerator FallingCoroutine()
    {
        yield return new WaitUntil(() => playerRB.linearVelocity.y < 0);
        while (!isGrounded)
        {
            playerRB.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1) * Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }
    }
}
