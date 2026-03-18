using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public event EventHandler OnAttack;
    public event EventHandler OnJump;

    private InputSystem_Actions inputActions;

    private Vector2 movementInput;

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
            inputActions.Player.Attack.performed += _ => OnAttack?.Invoke(this, EventArgs.Empty);
            inputActions.Player.Jump.performed += _ => OnJump?.Invoke(this, EventArgs.Empty);
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
