using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private readonly float DefaultSensitivity = 0.05f, ShakeMultiplier = 1f;

    private readonly float StandUpHeight = 1.75f, CrouchAmount = -0.8f;
    private readonly float MaxUpLookAngle = 75f, MaxDownLookAngle = 75f;
    private readonly float CrouchSharpness = 10f;
    private readonly float RunBobCycleDistance = 5f, CrouchBobCycleDistance = 2.75f, BobCycleResetSharpness = 7f, MaxBobRunSpeed = 12f;
    private readonly float RunVerticalBobDistance = 0.02f, RunHorizontalBobDistance = 0.01f;
    private readonly float CrouchVerticalBobDistance = 0.01f, CrouchHorizontalBobDistance = 0.005f;
    private readonly float GetHitAngleMax = 20f, GetHitDownTime = 0.1f, GetHitBackUpTime = 1f;

    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private GameObject root, shakeReference;

    private PlayerArms playerArms;

    private Vector2 currentLook;
    private Vector3 currentRootPosition;
    private Vector3 currentRootOffset;
    private Vector3 currentRootRotationOffset;

    private float targetCrouchAmount, currentCrouchAmount;
    private float headBobRunSpeed, currentCycleDistance;
    private Vector3 currentRunOffset;
    private bool crouching;
    private Vector2[] AttackOffsets = new Vector2[20];

    private float currentSensitivity;

    private bool halfCompleted;

    private void Awake()
    {
        playerArms = GetComponent<PlayerArms>();
    }

    private void OnEnable()
    {
        GameplayEvents.OnPlayerGotHit += GotHit;
        GameplayEvents.OnCutTree += TreeCutShake;
        UIEvents.OnSensitivityChanged += SetSensitivity;
    }

    private void OnDisable()
    {
        GameplayEvents.OnPlayerGotHit -= GotHit;
        GameplayEvents.OnCutTree -= TreeCutShake;
        UIEvents.OnSensitivityChanged -= SetSensitivity;
    }

    private void SetSensitivity(float newSensitivity)
    {
        currentSensitivity = newSensitivity / 10f;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        currentRootPosition = Vector3.up * StandUpHeight;
        currentCrouchAmount = 0;
        currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 10f) / 10f;
    }

    private void Update()
    {
        CalculateRootPosition();
        CalculateRootOffset();
        CalculateRootRotationOffset();
        
        root.transform.localPosition = currentRootPosition + currentRootOffset;
        root.transform.localRotation = Quaternion.Euler(new Vector3(-currentLook.y, currentLook.x, 0) + currentRootRotationOffset)
            * shakeReference.transform.localRotation;


        void CalculateRootPosition()
        {
            currentCrouchAmount = Mathf.Lerp(currentCrouchAmount, targetCrouchAmount, 1f - Mathf.Exp(-CrouchSharpness * Time.deltaTime));
            currentRootPosition = Vector3.up * (StandUpHeight + currentCrouchAmount);
        }

        void CalculateRootOffset()
        {
            const float TWOPI = Mathf.PI * 2;

            if (headBobRunSpeed > 0)
            {
                if (crouching)
                    currentCycleDistance += headBobRunSpeed * Time.deltaTime / CrouchBobCycleDistance * TWOPI;
                else
                    currentCycleDistance += headBobRunSpeed * Time.deltaTime / RunBobCycleDistance * TWOPI;

                if (!halfCompleted && currentCycleDistance > TWOPI / 2)
                {
                    halfCompleted = true;
                    GameplayEvents.PlayerStepEvent();
                }

                if (currentCycleDistance > TWOPI)
                {
                    halfCompleted = false;
                    GameplayEvents.PlayerStepEvent();
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

            if (crouching)
            {
                currentRunOffset.x = Mathf.Sin(currentCycleDistance) * CrouchHorizontalBobDistance;
                currentRunOffset.y = (Mathf.Pow(Mathf.Cos(currentCycleDistance), 2f) - 1) * CrouchVerticalBobDistance * 2;
            }
            else
            {
                currentRunOffset.x = Mathf.Sin(currentCycleDistance) * RunHorizontalBobDistance;
                currentRunOffset.y = (Mathf.Pow(Mathf.Cos(currentCycleDistance), 2f) - 1) * RunVerticalBobDistance * 2;
            }

            playerArms.SetRunOffset(currentRunOffset);
            currentRootOffset = Quaternion.AngleAxis(currentLook.x, Vector3.up) * currentRunOffset;
        }

        void CalculateRootRotationOffset()
        {
            currentRootRotationOffset = Vector3.zero;
            for (int i = 0; i < AttackOffsets.Length; i++)
            {
                currentRootRotationOffset.x += AttackOffsets[i].x;
                currentRootRotationOffset.y += AttackOffsets[i].y;
            }
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

    public void CrouchStart()
    {
        targetCrouchAmount = CrouchAmount;
        crouching = true;
    }

    public void CrouchStop()
    {
        targetCrouchAmount = 0;
        crouching = false;
    }

    public void SetHeadBob(float speed)
    {
        if (speed < MaxBobRunSpeed)
            headBobRunSpeed = speed;
        else
            headBobRunSpeed = MaxBobRunSpeed;
    }

    private void GotHit(float damage, Vector3 location)
    {
        int index;

        for (index = 0; index < AttackOffsets.Length; index++)
        {
            if (AttackOffsets[index] == Vector2.zero)
                break;
        }

        DOVirtual.Float(0, -GetHitAngleMax, GetHitDownTime, SetOffset).SetEase(Ease.OutCubic).OnComplete(GetDown);

        void GetDown()
        {
            DOVirtual.Float(-GetHitAngleMax, 0, GetHitBackUpTime, SetOffset).SetEase(Ease.InOutSine);
        }

        void SetOffset(float value)
        {
            AttackOffsets[index] = Vector2.left * value;
        }
    }

    public void RopeAttachShake()
    {
        ShakeNow(0.2f, 0.8f);
    }

    private void TreeCutShake(GameObject tree)
    {
        ShakeNow(0.2f, 2f);
    }

    private void ShakeNow(float duration, float strength)
    {
        shakeReference.transform.DOShakeRotation(duration, strength * ShakeMultiplier, 35, 90f, true, ShakeRandomnessMode.Harmonic).OnComplete(ResetShakeRotation);
    }

    private void ResetShakeRotation()
    {
        shakeReference.transform.localRotation = Quaternion.identity;
    }
}
