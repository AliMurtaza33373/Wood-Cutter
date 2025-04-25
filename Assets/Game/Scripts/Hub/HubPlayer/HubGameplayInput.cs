using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HubGameplayInput : MonoBehaviour
{
    private GameActions.GameplayActions gameplayActions;

    private HubPlayerCamera playerCamera;
    private HubPlayerMovement playerMovement;


    private void Awake()
    {
        playerCamera = GetComponent<HubPlayerCamera>();
        playerMovement = GetComponent<HubPlayerMovement>();
        gameplayActions = GameInput.gameActions.Gameplay;
    }

    private void OnEnable()
    {
        gameplayActions.Enable();
        gameplayActions.UseEnergy.performed += TriedToPressButton;
        gameplayActions.Pause.performed += PressedPause;
    }

    private void OnDisable()
    {
        gameplayActions.Disable();
        gameplayActions.UseEnergy.performed -= TriedToPressButton;
        gameplayActions.Pause.performed -= PressedPause;
    }

    private void PressedPause(InputAction.CallbackContext callbackContext)
    {
        HubManager.Instance.PauseUnPause();
    }

    private void TriedToPressButton(InputAction.CallbackContext callbackContext)
    {
        if (HubManager.Instance.IsGamePaused)
            return;

        if (HubManager.Instance.platform != null)
            HubManager.Instance.platform.AttemptPressButton();
    }

    private void Update()
    {
        if (HubManager.Instance.IsGamePaused)
            return;

        playerCamera.SetLookInput(gameplayActions.Look.ReadValue<Vector2>());
        playerMovement.SetMovementInput(gameplayActions.Run.ReadValue<Vector2>());
    }
}
