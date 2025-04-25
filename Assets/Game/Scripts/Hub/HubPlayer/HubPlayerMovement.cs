using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HubPlayerMovement : MonoBehaviour, ICharacterController
{
    private readonly float RunSpeed = 6f, RunAcceleration = 40f, RunDeceleration = 40f, AirAcceleration = 8f;
    private readonly float Gravity = 20f;

    public Vector3 CurrentPlayerVelocity { get { return previousVelocity; } }
    public bool IsOnGround { get { return motor.GroundingStatus.IsStableOnGround; } }


    [SerializeField]
    private KinematicCharacterMotor motor;

    private HubPlayerCamera playerCamera;

    private Vector3 movementInput;
    private Vector3 previousVelocity;


    private void Awake()
    {
        motor.CharacterController = this;
        playerCamera = GetComponent<HubPlayerCamera>();
    }

    private void Start()
    {
        movementInput = Vector3.zero;
        previousVelocity = Vector3.zero;
    }

    public void SetMovementInput(Vector2 input)
    {
        movementInput.x = input.x;
        movementInput.z = input.y;
        movementInput = Quaternion.AngleAxis(playerCamera.GetCurrentLook().x, motor.CharacterUp) * movementInput;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 targetVelocity = currentVelocity;

        float headBobRunSpeed = 0f;

        // ground movement
        if (motor.GroundingStatus.IsStableOnGround)
        {
            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;
            Vector3 slopeDirectionComponent = new Vector3(effectiveGroundNormal.x, 0, effectiveGroundNormal.z);
            Vector3 slopeDirection = Vector3.Cross(effectiveGroundNormal, Vector3.Cross(slopeDirectionComponent.normalized, motor.CharacterUp)).normalized;

            float currentVelocityMagnitude = currentVelocity.magnitude;
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, Vector3.Cross(movementInput, motor.CharacterUp)).normalized * movementInput.magnitude;


            // apply deceleration
            if (currentVelocityMagnitude > RunDeceleration * deltaTime)
                targetVelocity = targetVelocity - (targetVelocity.normalized * RunDeceleration * deltaTime);
            else
                targetVelocity = Vector3.zero;

            // input
            if (movementInput.sqrMagnitude > 0)
            {
                Vector3 inputTargetVelocity = targetVelocity + ((RunAcceleration + RunDeceleration) * deltaTime * reorientedInput);

                if (targetVelocity.magnitude < RunSpeed)
                {
                    if (inputTargetVelocity.magnitude > RunSpeed)
                        inputTargetVelocity = inputTargetVelocity.normalized * RunSpeed;
                }
                else
                    inputTargetVelocity = inputTargetVelocity.normalized * targetVelocity.magnitude;

                targetVelocity = inputTargetVelocity;
            }

            headBobRunSpeed = targetVelocity.magnitude;
        }
        // air movement
        else
        {
            // input
            if (movementInput.sqrMagnitude > 0f)
            {
                Vector3 currentHorizontalVelocity = Vector3.ProjectOnPlane(targetVelocity, motor.CharacterUp);
                Vector3 targetHorizontalVelocity = (movementInput * AirAcceleration * deltaTime) + currentHorizontalVelocity;

                if (currentHorizontalVelocity.magnitude >= RunSpeed)
                {
                    if (targetHorizontalVelocity.magnitude > currentHorizontalVelocity.magnitude)
                        targetHorizontalVelocity = targetHorizontalVelocity.normalized * currentHorizontalVelocity.magnitude;
                }
                else if (targetHorizontalVelocity.magnitude > RunSpeed)
                    targetHorizontalVelocity = targetHorizontalVelocity.normalized * RunSpeed;

                targetVelocity = new Vector3(targetHorizontalVelocity.x, targetVelocity.y, targetHorizontalVelocity.z);
            }

            targetVelocity += Vector3.down * Gravity * deltaTime;
        }

        currentVelocity = targetVelocity;

        playerCamera.SetHeadBob(headBobRunSpeed);
        previousVelocity = currentVelocity;
    }



    // unused methods
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
    Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) { }
    public void BeforeCharacterUpdate(float deltaTime) { }
    public bool IsColliderValidForCollisions(Collider coll) { return true; }
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void AfterCharacterUpdate(float deltaTime) { }
    public void PostGroundingUpdate(float deltaTime) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
}
