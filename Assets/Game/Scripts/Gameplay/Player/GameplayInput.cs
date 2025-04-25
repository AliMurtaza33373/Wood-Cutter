using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayInput : MonoBehaviour
{
    private GameActions.GameplayActions gameplayActions;

    private PlayerCamera playerCamera;
    private PlayerMovement playerMovement;
    private PlayerArms playerArms;
    private PlayerGadgets playerGadgets;

    private void Awake()
    {
        playerCamera = GetComponent<PlayerCamera>();
        playerMovement = GetComponent<PlayerMovement>();
        playerArms = GetComponent<PlayerArms>();
        playerGadgets = GetComponent<PlayerGadgets>();
        gameplayActions = GameInput.gameActions.Gameplay;
    }

    private void OnEnable()
    {
        gameplayActions.Enable();
        gameplayActions.Jump.performed += JumpPerformed;
        gameplayActions.Attack.performed += StartedHoldingAttack;
        gameplayActions.Reset.performed += ResetPerformed;
        gameplayActions.Pause.performed += PressedPause;
    }

    private void OnDisable()
    {
        gameplayActions.Disable();
        gameplayActions.Jump.performed -= JumpPerformed;
        gameplayActions.Attack.performed -= StartedHoldingAttack;
        gameplayActions.Reset.performed -= ResetPerformed;
        gameplayActions.Pause.performed -= PressedPause;
    }

    private void JumpPerformed(InputAction.CallbackContext callbackContext)
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsGamePaused)
            return;

        playerMovement.PressedJump();
    }


    private void StartedHoldingAttack(InputAction.CallbackContext callbackContext)
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsGamePaused)
            return;

        GameplayEvents.PlayerPressLeftMouseEvent();
        playerArms.StartAttack();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ResetPerformed(InputAction.CallbackContext callbackContext)
    {
        GameplayEvents.GameResetEvent();
    }

    private void PressedPause(InputAction.CallbackContext callbackContext)
    {
        if (GameManager.Instance.IsGameOver)
            return;

        GameManager.Instance.PauseUnPause();
    }


    private void Update()
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsGamePaused)
            return;

        playerCamera.SetLookInput(gameplayActions.Look.ReadValue<Vector2>());
        playerMovement.SetMovementInput(gameplayActions.Run.ReadValue<Vector2>());
        playerMovement.SetCrouchHold(gameplayActions.Crouch.inProgress);
        playerGadgets.SetRopePressed(gameplayActions.Rope.inProgress);
    }
}
