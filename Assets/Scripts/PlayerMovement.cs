using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 moveDirection;
    private Vector2 moveInput;
    private float verticalInput;
    private float horizontalInput;
    
    private Transform cameraPOV;
    private Rigidbody playerRB;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
        cameraPOV = Camera.main.transform;
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
        moveDirection = cameraPOV.forward * verticalInput;
        moveDirection += cameraPOV.right * horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;
        moveDirection *= movementSpeed;

        Vector3 moveVelocity = moveDirection;
        playerRB.linearVelocity = moveVelocity;
    }

    private void HandleRotation()
    {
        Vector3 targetDirection;

        targetDirection = cameraPOV.forward * verticalInput;
        targetDirection += targetDirection + cameraPOV.right * horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }
}
