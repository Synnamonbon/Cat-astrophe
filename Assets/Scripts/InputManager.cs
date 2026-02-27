using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    private InputSystem_Actions inputActions;

    public Vector2 movementInput;

    private void Awake()
    {
        SingletonPattern();
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

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            
            inputActions.Player.Move.performed += i => movementInput = i.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += i => movementInput = Vector2.zero;
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
