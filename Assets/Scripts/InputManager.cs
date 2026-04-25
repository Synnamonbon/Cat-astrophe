using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public event EventHandler OnAttack;
    public event EventHandler OnJump;
    public event EventHandler OnMeow;
    public event EventHandler OnInteract;

    private InputSystem_Actions inputActions;

    private Vector2 movementInput;
    public bool isSprinting {get; private set;}

    private void Awake()
    {
        SingletonPattern();
        DontDestroyOnLoad(gameObject);
    }

    private void SingletonPattern()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnDestroy()
    {
        OnAttack = null;
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            
            inputActions.Player.Move.performed += i => movementInput = i.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += i => movementInput = Vector2.zero;
            inputActions.Player.Sprint.started += _ => isSprinting = true;
            inputActions.Player.Sprint.canceled += _ => isSprinting = false;
            inputActions.Player.Attack.performed += _ => OnAttack?.Invoke(this, EventArgs.Empty);
            inputActions.Player.Jump.performed += _ => OnJump?.Invoke(this, EventArgs.Empty);
            inputActions.Player.Meow.performed += _ => OnMeow?.Invoke(this, EventArgs.Empty);
            inputActions.Player.Interact.performed += _ => OnInteract?.Invoke(this, EventArgs.Empty);
        }

        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public Vector2 HandleMovementInput()
    {
        Vector2 moveVector = movementInput;
        return moveVector;
    }
}
