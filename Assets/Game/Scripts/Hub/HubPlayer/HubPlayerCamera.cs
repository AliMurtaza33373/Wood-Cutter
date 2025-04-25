using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class HubPlayerCamera : MonoBehaviour
{
    private readonly float DefaultSensitivity = 0.05f;

    private readonly float StandUpHeight = 1.75f;
    private readonly float MaxUpLookAngle = 75f, MaxDownLookAngle = 75f;
    private readonly float RunBobCycleDistance = 5f, BobCycleResetSharpness = 7f, MaxBobRunSpeed = 12f;
    private readonly float RunVerticalBobDistance = 0.015f, RunHorizontalBobDistance = 0.0075f;


    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private GameObject root;

    private Vector2 currentLook;
    private Vector3 currentRootPosition;
    private Vector3 currentRootOffset;

    private float currentJumpOffset, currentLandOffset, currentLandOffsetSpeed;
    private float headBobRunSpeed, currentCycleDistance;
    private Vector3 currentRunOffset;

    private float currentSensitivity;

    private bool halfCompleted;

    private void OnEnable()
    {
        UIEvents.OnSensitivityChanged += SetSensitivity;
    }

    private void OnDisable()
    {
        UIEvents.OnSensitivityChanged -= SetSensitivity;
    }

    private void SetSensitivity(float newSensitivity)
    {
        currentSensitivity = newSensitivity / 10f;
    }

    private void Start()
    {
        // temp code
        Cursor.lockState = CursorLockMode.Locked;
        currentRootPosition = Vector3.up * StandUpHeight;
        currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 10f) / 10f;
    }

    private void Update()
    {
        CalculateRootOffset();
        
        root.transform.localPosition = currentRootPosition + currentRootOffset;
        root.transform.localRotation = Quaternion.Euler(new Vector3(-currentLook.y, currentLook.x, 0));

        void CalculateRootOffset()
        {
            if (currentLandOffsetSpeed < 0)
                currentLandOffset += currentLandOffsetSpeed * Time.deltaTime;

            const float TWOPI = Mathf.PI * 2;

            if (headBobRunSpeed > 0)
            {
                currentCycleDistance += headBobRunSpeed * Time.deltaTime / RunBobCycleDistance * TWOPI;

                if (!halfCompleted && currentCycleDistance > TWOPI / 2)
                {
                    halfCompleted = true;
                    HubEvents.PlayerStepEvent();
                }

                if (currentCycleDistance > TWOPI)
                {
                    halfCompleted = false;
                    HubEvents.PlayerStepEvent();
                    currentCycleDistance -= TWOPI;
                }
            }
            else
            {
                // ease back to the closest zero
                if (currentCycleDistance < Mathf.PI * 0.5f)
                    currentCycleDistance = Mathf.Lerp(currentCycleDistance, 0, 1f - Mathf.Exp(-BobCycleResetSharpness * Time.deltaTime));
                else if (currentCycleDistance < Mathf.PI * 1.5f)
                    currentCycleDistance = Mathf.Lerp(currentCycleDistance, Mathf.PI, 1f - Mathf.Exp(-BobCycleResetSharpness * Time.deltaTime));
                else
                    currentCycleDistance = Mathf.Lerp(currentCycleDistance, TWOPI, 1f - Mathf.Exp(-BobCycleResetSharpness * Time.deltaTime));
            }

            currentRunOffset.x = Mathf.Sin(currentCycleDistance) * RunHorizontalBobDistance;
            currentRunOffset.y = (Mathf.Pow(Mathf.Cos(currentCycleDistance), 2f) - 1) * RunVerticalBobDistance * 2;

            currentRootOffset = Quaternion.AngleAxis(currentLook.x, Vector3.up) * currentRunOffset;
            currentRootOffset += Vector3.up * (currentJumpOffset + currentLandOffset);
        }
    }

    public void SetLookInput(Vector2 input)
    {
        currentLook += input * DefaultSensitivity * currentSensitivity;

        if (currentLook.y > MaxUpLookAngle)
        {
            currentLook.y = MaxUpLookAngle;
        }
        else if (currentLook.y < -MaxDownLookAngle)
        {
            currentLook.y = -MaxDownLookAngle;
        }

        if (currentLook.x > 180)
        {
            currentLook.x -= 360;
        }
        else if (currentLook.x < -180)
        {
            currentLook.x += 360;
        }
    }

    public Vector2 GetCurrentLook()
    {
        return currentLook;
    }

    public void SetHeadBob(float speed)
    {
        if (speed < MaxBobRunSpeed)
            headBobRunSpeed = speed;
        else
            headBobRunSpeed = MaxBobRunSpeed;
    }
}
