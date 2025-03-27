using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    // player input is a component attached to the object
    public static PlayerInput PlayerInput;

    private InputAction moveInputAction;
    private InputAction brakeAction;
    private InputAction handBrakeAction;
    private InputAction menuOpenAction;
    private InputAction menuCloseAction;

    public Vector2 moveVector { get; private set; }
    public bool isBrake{ get; private set; }
    public bool isHandBrake { get; private set; }
    public bool MenuOpenInput { get; private set; }
    public bool MenuCloseInput { get; private set; }

    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
        }

        // get component from PlayerInput asset in inspector
        PlayerInput = GetComponent<PlayerInput>();

        // player actions
        moveInputAction = PlayerInput.actions["Movement"];
        brakeAction = PlayerInput.actions["Brake"];
        handBrakeAction = PlayerInput.actions["HandBrake"];
        menuOpenAction = PlayerInput.actions["MenuOpen"];
        menuCloseAction = PlayerInput.actions["MenuClose"];
    }

    private void Update()
    {
        // get player actions
        moveVector = moveInputAction.ReadValue<Vector2>();
        isBrake = brakeAction.IsPressed();
        isHandBrake = handBrakeAction.IsPressed();

        // get menu actions
        MenuOpenInput = menuOpenAction.WasPressedThisFrame();
        MenuCloseInput = menuCloseAction.WasPressedThisFrame();
    }
}
