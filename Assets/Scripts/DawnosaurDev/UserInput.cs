using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput instance;

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpReleased { get; private set; }
    public bool AttackInput { get; private set; }
    public bool SpecialInput { get; private set; }
    public bool InteractInput { get; private set; }
    public bool SprintInput { get; private set; }

    public bool SprintHeld { get; private set; }
    public bool MenuInput { get; private set; }
    public bool MapInput { get; private set; }
    public bool InventoryInput { get; private set; }

    public Vector2 AimInput { get; private set; }

    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;

    private InputAction _attackAction;
    private InputAction _specialAction;
    private InputAction _moveAction;
    private InputAction _mouseAction;
    private InputAction _aimAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _interactAction;
    private InputAction _menuAction;
    private InputAction _mapAction;
    private InputAction _inventoryAction;
    private InputAction _leftAction;


    Vector2 _lastMouseVector = Vector2.right;
    Vector2 _lastGamepadVector = Vector2.right;
    public bool mouseActive = true;



    private float moveCooldown = 5;

    public float LastMouseMoveTime { get; private set; }

    public float LastRightMoveTime { get; private set; }
    public float LastLeftMoveTime { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;

        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMovement>();

        SetupInputActions();

    }

    private void SetupInputActions()
    {
        _attackAction = _playerInput.actions["Attack"];
        _specialAction = _playerInput.actions["Special"];
        _jumpAction = _playerInput.actions["Jump"];
        _sprintAction = _playerInput.actions["Sprint"];
        _interactAction = _playerInput.actions["Interact"];
        _menuAction = _playerInput.actions["Menu"];
        _mapAction = _playerInput.actions["Map"];
        _inventoryAction = _playerInput.actions["Inventory"];
        _moveAction = _playerInput.actions["Move"];
        _mouseAction = _playerInput.actions["Cursor"];
        _aimAction = _playerInput.actions["AimDirection"];
        _leftAction = _playerInput.actions["LeftStick"];
    }

    // Update is called once per frame
    void Update()
    {

        Console.WriteLine("Updating");
        LastLeftMoveTime -= Time.deltaTime;
        LastRightMoveTime -= Time.deltaTime;
        LastMouseMoveTime -= Time.deltaTime;

        MoveInput = _moveAction.ReadValue<Vector2>();
        JumpPressed = _jumpAction.WasPerformedThisFrame();
        JumpHeld = _jumpAction.IsPressed();
        JumpReleased = _jumpAction.WasReleasedThisFrame();

        SprintInput = _sprintAction.WasPressedThisFrame();
        SprintHeld = _sprintAction.IsPressed();

        AttackInput = _attackAction.WasPerformedThisFrame();
        SpecialInput = _specialAction.WasPerformedThisFrame();
        InteractInput = _interactAction.WasPerformedThisFrame();

        Vector2 location = _mouseAction.ReadValue<Vector2>();

        Vector2 mouseDirection = new Vector2(location.x - Screen.width / 2, location.y - Screen.height / 2).normalized;
        Vector2 gamepadDirection = _aimAction.ReadValue<Vector2>().normalized;

        Vector2 leftInput = _leftAction.ReadValue<Vector2>();

        if (Vector2.Distance(leftInput, Vector2.zero) > 0.1f)
        {
            mouseActive = false;
            LastLeftMoveTime = moveCooldown;
        }

        if (Vector2.Distance(gamepadDirection, Vector2.zero) > 0.1f && Vector2.Distance(_lastGamepadVector, gamepadDirection) > 0.01f)
        {
            _lastGamepadVector = gamepadDirection;
            mouseActive = false;
            LastRightMoveTime = moveCooldown;
        }
        else if (Vector2.Distance(_lastMouseVector, mouseDirection) > 0.01f)
        {
            _lastMouseVector = mouseDirection;
            mouseActive = true;
            LastMouseMoveTime = moveCooldown;
        }

        AimInput = mouseActive ? _lastMouseVector : _lastGamepadVector;
    }
}
