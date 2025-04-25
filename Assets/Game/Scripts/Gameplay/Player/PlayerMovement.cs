using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour, ICharacterController
{
    private readonly float RunSpeed = 8f, RunAcceleration = 40f, RunDeceleration = 40f, AirAcceleration = 8f;
    private readonly float JumpPostGroundingGraceTime = 0.2f, JumpPreGroundingGraceTime = 0.2f, JumpUpSpeed = 8f;
    private readonly float CrouchHeight = 1.2f, CrouchAcceleration = 15f, CrouchDeceleration = 15f;
    private readonly float SlideMinSpeed = 4f, CrouchMaxSpeed = 3f, StartSlideBonusTime = 0.5f, AfterSlideLandLimitTime = 1f, SlideDeceleration = 4f, SlideAcceleration = 3f;
    private readonly float MinimumSlopeSlideAngle = 5f, SlopeSlideMultiplier = 15f, SlopeLandAbsorbMultiplier = 0.65f;
    private readonly float MaxNormalSnappingAngleChange = 180f, MaxSlidingSnappingAngleChange = 1f;
    private readonly float RopeMinlength = 5f, SwingAcceleration = 8f;
    private readonly float MaxHorizontalSpeedMultipler = 1.75f, Gravity = 20f;
    private readonly float GetHitSpeedReduceTime = 2f, GetHitMaxSpeed = 2f;
    private readonly float FlowerUpSpeed = 16f;

    public Vector3 CurrentPlayerVelocity { get { return previousVelocity; } }
    public bool IsCrouching { get; private set; }
    public bool AnticipatingJump { get; private set; }

    [SerializeField]
    private KinematicCharacterMotor motor;

    private PlayerCamera playerCamera;

    private Vector3 movementInput;
    private Vector3 previousVelocity, externalVelocityToAdd;

    private float defaultHeight, landingSpeed;

    private bool jumpRequested, jumpConsumed, jumpingRightNow;
    private float timeSinceJumpRequested, timeRemainJumpAnticipate, timeSinceLastAbleToJump, timeRemainHitRecover;

    private bool shouldBeCrouching, jumpedDespiteSlideLimit;
    private float timeRemainSlideBonusTime, timeRemainSlideLandTime;
    private float currentRopeDistance;

    private Collider[] probedColliders = new Collider[8];

    private void Awake()
    {
        motor.CharacterController = this;
        playerCamera = GetComponent<PlayerCamera>();
    }

    private void OnEnable()
    {
        GameplayEvents.OnPlayerGotHit += ForceStopPlayer;
    }

    private void OnDisable()
    {
        GameplayEvents.OnPlayerGotHit -= ForceStopPlayer;
    }

    private void Start()
    {
        movementInput = Vector3.zero;
        previousVelocity = Vector3.zero;
        jumpRequested = false;
        AnticipatingJump = false;
        jumpConsumed = false;
        jumpingRightNow = false;
        jumpedDespiteSlideLimit = false;
        timeSinceJumpRequested = Mathf.Infinity;
        timeSinceLastAbleToJump = 0f;
        defaultHeight = motor.Capsule.height;
        landingSpeed = 0f;
        timeRemainHitRecover = 0f;
    }

    public void SetMovementInput(Vector2 input)
    {
        movementInput.x = input.x;
        movementInput.z = input.y;
        movementInput = Quaternion.AngleAxis(playerCamera.GetCurrentLook().x, motor.CharacterUp) * movementInput;
    }

    public void PressedJump()
    {
        timeSinceJumpRequested = 0f;
        jumpRequested = true;
    }

    public void SetCrouchHold(bool holdingCrouch)
    {
        if (!motor.GroundingStatus.IsStableOnGround)
            return;

        if (holdingCrouch)
        {
            shouldBeCrouching = true;

            if (!IsCrouching)
            {
                IsCrouching = true;
                timeRemainSlideBonusTime = StartSlideBonusTime;
                motor.SetCapsuleDimensions(motor.Capsule.radius, CrouchHeight, CrouchHeight * 0.5f);
                playerCamera.CrouchStart();
            }
        }
        else
            shouldBeCrouching = false;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 targetVelocity = currentVelocity;

        float headBobRunSpeed = 0f;

        if (!PlayerInteractions.Instance.IsRopeAttached)
            currentRopeDistance = 0f;
        else
        {
            currentRopeDistance = Vector3.Distance(transform.position, PlayerInteractions.Instance.CurrentRopeTarget.point);
            if (currentRopeDistance < RopeMinlength)
                currentRopeDistance = RopeMinlength;
        }

        // ground movement
        if (motor.GroundingStatus.IsStableOnGround)
        {
            if (timeRemainSlideLandTime > 0f)
                timeRemainSlideLandTime -= deltaTime;

            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;
            Vector3 slopeDirectionComponent = new Vector3(effectiveGroundNormal.x, 0, effectiveGroundNormal.z);
            Vector3 slopeDirection = Vector3.Cross(effectiveGroundNormal, Vector3.Cross(slopeDirectionComponent.normalized, motor.CharacterUp)).normalized;
            bool onSlope = Vector3.Angle(motor.CharacterUp, effectiveGroundNormal) > MinimumSlopeSlideAngle;

            // landing on slope
            if (onSlope && landingSpeed != 0)
            {
                targetVelocity += landingSpeed * slopeDirection.normalized * slopeDirectionComponent.magnitude * SlopeLandAbsorbMultiplier;
                currentVelocity = targetVelocity;
                landingSpeed = 0;
            }

            float currentVelocityMagnitude = currentVelocity.magnitude;
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, Vector3.Cross(movementInput, motor.CharacterUp)).normalized * movementInput.magnitude;

            // sliding
            if ((IsCrouching && currentVelocityMagnitude > SlideMinSpeed) || PlayerInteractions.Instance.IsRopeAttached)
            {
                if (motor.MaxStableDenivelationAngle != MaxSlidingSnappingAngleChange)
                    motor.MaxStableDenivelationAngle = MaxSlidingSnappingAngleChange;

                // slide down slope
                if (onSlope)
                    targetVelocity += slopeDirection.normalized * slopeDirectionComponent.magnitude * SlopeSlideMultiplier * deltaTime;

                float beforeDecelerateMagnitude = targetVelocity.magnitude;

                // apply deceleration
                if (currentVelocityMagnitude > SlideDeceleration * deltaTime)
                    targetVelocity = targetVelocity - (targetVelocity.normalized * SlideDeceleration * deltaTime);
                else
                    targetVelocity = Vector3.zero;

                // slide bonus time
                if (timeRemainSlideBonusTime > 0)
                {
                    if (targetVelocity.magnitude < currentVelocityMagnitude && beforeDecelerateMagnitude >= currentVelocityMagnitude)
                        targetVelocity = targetVelocity.normalized * currentVelocityMagnitude;

                    timeRemainSlideBonusTime -= deltaTime;
                }

                // input
                if (movementInput.sqrMagnitude > 0f)
                {
                    Vector3 slidingTargetVelocity = (reorientedInput * (SlideAcceleration + SlideDeceleration) * deltaTime) + targetVelocity;

                    if (slidingTargetVelocity.magnitude > targetVelocity.magnitude)
                        targetVelocity = slidingTargetVelocity.normalized * targetVelocity.magnitude;
                    else
                        targetVelocity = slidingTargetVelocity;
                }
            }
            else
            {
                if (motor.MaxStableDenivelationAngle != MaxNormalSnappingAngleChange)
                    motor.MaxStableDenivelationAngle = MaxNormalSnappingAngleChange;

                float currentMaxSpeed, currentAcceleration, currentDeceleration;

                // crouching
                if (IsCrouching)
                {
                    timeRemainSlideBonusTime = 0;
                    currentMaxSpeed = CrouchMaxSpeed;
                    currentAcceleration = CrouchAcceleration;
                    currentDeceleration = CrouchDeceleration;
                }
                // running
                else
                {
                    currentMaxSpeed = RunSpeed;
                    currentAcceleration = RunAcceleration;
                    currentDeceleration = RunDeceleration;
                }

                // if got hit recently, then limit speed
                if (timeRemainHitRecover > 0)
                {
                    timeRemainHitRecover -= deltaTime;
                    currentMaxSpeed = GetHitMaxSpeed;
                }


                // apply deceleration
                if (currentVelocityMagnitude > currentDeceleration * deltaTime)
                    targetVelocity = targetVelocity - (targetVelocity.normalized * currentDeceleration * deltaTime);
                else
                    targetVelocity = Vector3.zero;

                // input
                if (movementInput.sqrMagnitude > 0)
                {
                    Vector3 inputTargetVelocity = targetVelocity + ((currentAcceleration + currentDeceleration) * deltaTime * reorientedInput);

                    if (targetVelocity.magnitude < currentMaxSpeed)
                    {
                        if (inputTargetVelocity.magnitude > currentMaxSpeed)
                            inputTargetVelocity = inputTargetVelocity.normalized * currentMaxSpeed;
                    }
                    else
                        inputTargetVelocity = inputTargetVelocity.normalized * targetVelocity.magnitude;

                    targetVelocity = inputTargetVelocity;
                }

                headBobRunSpeed = targetVelocity.magnitude;
            }

            landingSpeed = 0;
        }
        // air movement
        else
        {
            // swinging motion
            if (PlayerInteractions.Instance.IsRopeAttached)
                targetVelocity += movementInput * SwingAcceleration * deltaTime;
            else
            {
                // input
                if (movementInput.sqrMagnitude > 0f)
                {
                    Vector3 currentHorizontalVelocity = Vector3.ProjectOnPlane(targetVelocity, motor.CharacterUp);
                    Vector3 targetHorizontalVelocity = (movementInput * AirAcceleration * deltaTime) + currentHorizontalVelocity;

                    if (currentHorizontalVelocity.magnitude >= CrouchMaxSpeed)
                    {
                        if (targetHorizontalVelocity.magnitude > currentHorizontalVelocity.magnitude)
                            targetHorizontalVelocity = targetHorizontalVelocity.normalized * currentHorizontalVelocity.magnitude;
                    }
                    else if (targetHorizontalVelocity.magnitude > CrouchMaxSpeed)
                        targetHorizontalVelocity = targetHorizontalVelocity.normalized * CrouchMaxSpeed;

                    targetVelocity = new Vector3(targetHorizontalVelocity.x, targetVelocity.y, targetHorizontalVelocity.z);
                }

                // slideLimit
                if (jumpedDespiteSlideLimit)
                {
                    Vector3 NoHeightComponent = Vector3.ProjectOnPlane(targetVelocity, motor.CharacterUp);

                    if (NoHeightComponent.magnitude > SlideDeceleration * deltaTime)
                        NoHeightComponent = NoHeightComponent - (NoHeightComponent.normalized * SlideDeceleration * deltaTime);
                    else
                        NoHeightComponent = Vector3.zero;

                    targetVelocity.x = NoHeightComponent.x;
                    targetVelocity.z = NoHeightComponent.z;
                }
            }
            targetVelocity += Vector3.down * Gravity * deltaTime;
        }

        // swing movement
        if (PlayerInteractions.Instance.IsRopeAttached)
        {
            float targetRopeDistance = Vector3.Distance(transform.position + (targetVelocity * deltaTime), PlayerInteractions.Instance.CurrentRopeTarget.point);
            if (targetRopeDistance > currentRopeDistance)
            {
                Vector3 ropeAttachPointDirection = transform.position + (targetVelocity * deltaTime) - PlayerInteractions.Instance.CurrentRopeTarget.point;
                targetVelocity = (PlayerInteractions.Instance.CurrentRopeTarget.point + (ropeAttachPointDirection.normalized * currentRopeDistance) - transform.position) / deltaTime;
            }

            if (targetVelocity.magnitude > RunSpeed * MaxHorizontalSpeedMultipler && targetVelocity.sqrMagnitude > currentVelocity.sqrMagnitude)
                targetVelocity = targetVelocity.normalized * currentVelocity.magnitude;

            if (motor.GroundingStatus.IsStableOnGround)
            {
                if (targetVelocity.y > 0)
                    motor.ForceUnground();
            }
        }

        currentVelocity = targetVelocity;

        // jumping
        if (AnticipatingJump)
        {
            if (timeRemainJumpAnticipate > 0)
                timeRemainJumpAnticipate -= deltaTime;
            else
            {
                // slide limit
                if (IsCrouching && timeRemainSlideLandTime > 0)
                {
                    jumpedDespiteSlideLimit = true;
                    timeRemainSlideBonusTime = 0;
                }

                GameplaySoundManager.Instance.PlayJump();
                AnticipatingJump = false;
                motor.ForceUnground();
                currentVelocity += (motor.CharacterUp * JumpUpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
            }
        }
        else
        {
            jumpingRightNow = false;
            timeSinceJumpRequested += deltaTime;
        }

        if (jumpRequested)
        {
            if (!jumpConsumed && (motor.GroundingStatus.IsStableOnGround || timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
            {
                timeRemainJumpAnticipate = 0;
                AnticipatingJump = true;
                jumpRequested = false;
                jumpConsumed = true;
                jumpingRightNow = true;
            }
        }
        else if (PlayerInteractions.Instance.IsRopeAttached)
        {
            if (!jumpConsumed && motor.GroundingStatus.IsStableOnGround)
            {
                timeRemainJumpAnticipate = 0;
                AnticipatingJump = true;
                jumpRequested = false;
                jumpConsumed = true;
                jumpingRightNow = true;
            }
        }

        // external forces
        if (externalVelocityToAdd.sqrMagnitude > 0)
        {
            motor.ForceUnground();
            currentVelocity += externalVelocityToAdd;
            externalVelocityToAdd = Vector3.zero;
        }

        playerCamera.SetHeadBob(headBobRunSpeed);
        previousVelocity = currentVelocity;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // jumping
        if (jumpRequested && timeSinceJumpRequested > JumpPreGroundingGraceTime)
            jumpRequested = false;

        if (motor.GroundingStatus.IsStableOnGround)
        {
            if (!jumpingRightNow)
                jumpConsumed = false;
            timeSinceLastAbleToJump = 0f;
        }
        else
            timeSinceLastAbleToJump += deltaTime;

        // crouching
        if (IsCrouching && !shouldBeCrouching)
        {
            // check if can stand up
            motor.SetCapsuleDimensions(motor.Capsule.radius, defaultHeight, defaultHeight / 2);
            if (motor.CharacterOverlap(
                motor.TransientPosition,
                motor.TransientRotation,
                probedColliders,
                motor.CollidableLayers,
                QueryTriggerInteraction.Ignore) > 0)
            {
                motor.SetCapsuleDimensions(motor.Capsule.radius, CrouchHeight, CrouchHeight * 0.5f);
            }
            else
            {
                playerCamera.CrouchStop();
                IsCrouching = false;
            }
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        if (motor.GroundingStatus.IsStableOnGround && !motor.LastGroundingStatus.IsStableOnGround)
        {
            GameplaySoundManager.Instance.PlayLand();

            if (IsCrouching)
                timeRemainSlideLandTime = AfterSlideLandLimitTime;
            jumpedDespiteSlideLimit = false;
            landingSpeed = -previousVelocity.y;
        }
    }

    private void ForceStopPlayer(float damage, Vector3 location)
    {
        timeRemainHitRecover = GetHitSpeedReduceTime;
    }

    public void FlowerExplodeUp()
    {
        externalVelocityToAdd += Vector3.up * (FlowerUpSpeed - previousVelocity.y);
    }


    // unused methods
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
    Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) { }
    public void BeforeCharacterUpdate(float deltaTime) { }
    public bool IsColliderValidForCollisions(Collider coll) { return true; }
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
}
