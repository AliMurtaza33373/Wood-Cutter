using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArms : MonoBehaviour
{
    private readonly float SlashStartingScale = 0.6f, SlashEndingScale = 0.98f, SlashRotationRange = 10f;
    private readonly float SlashAnticipationTime = 0.01f, SlashOccurTime = 0.1f, SlashRecoveryTime = 1f;
    private readonly float ShoulderBaseRotation = 10f, RightSideMaxRotation = 115f, LeftSideMaxRotation = -30f;

    public bool CurrentlySlashing { get; private set; }


    [SerializeField]
    private Transform root, swordHandTarget, swordAttackTarget, swordPivot;

    [SerializeField]
    private Transform ropeStartPoint;

    [SerializeField]
    private ParticleSystem swordTrail;

    private Vector3 defaultSwordPosition, currentSwordPosition;

    private Vector3 currentRunOffset;

    private Quaternion defaultSwordRotation, currentSwordRotation;

    private bool currentlyInSlashState, recovering;
    private bool attackDuringInterval, releaseDuringInterval;
    private float timeRemainSlashInterval;
    private Tween currentSwordPositionTween, currentSwordRotationTween;
    private Sequence swordAttackSequence;


    private void Start()
    {
        defaultSwordPosition = swordHandTarget.localPosition;
        defaultSwordRotation = swordHandTarget.localRotation;

        currentSwordPosition = defaultSwordPosition;
        currentSwordRotation = defaultSwordRotation;
    }

    private void Update()
    {
        if (timeRemainSlashInterval > 0)
            timeRemainSlashInterval -= Time.deltaTime;
        else if (attackDuringInterval)
        {
            attackDuringInterval = false;
            StartAttack();
            if (releaseDuringInterval)
                ReleaseAttack();
        }

        if (CurrentlySlashing)
        {
            currentSwordPosition = swordAttackTarget.localPosition;
            currentSwordRotation = swordAttackTarget.localRotation;
        }
        else
        {
            currentSwordPosition = defaultSwordPosition;
            currentSwordRotation = defaultSwordRotation;
        }

        Vector3 newPosition = currentRunOffset;
        swordHandTarget.localPosition = currentSwordPosition + newPosition;

        if (!recovering)
            swordHandTarget.localRotation = currentSwordRotation;
    }


    public void SetRunOffset(Vector3 offset)
    {
        offset.y *= 0.4f;
        offset.x *= 1.6f;
        currentRunOffset = offset;
    }

    public void StartAttack()
    {
        ReleaseAttack();
    }

    public void ReleaseAttack()
    {
        if (attackDuringInterval)
        {
            releaseDuringInterval = true;
            return;
        }

        if (currentlyInSlashState && !recovering)
            return;


        if (recovering)
            CancelSlash();

        if (gameObject != null && gameObject.activeInHierarchy)
            StartCoroutine(Slash());

        IEnumerator Slash()
        {
            currentlyInSlashState = true;

            timeRemainSlashInterval = SlashAnticipationTime + SlashOccurTime;

            float startRotation = Random.Range(-SlashRotationRange, SlashRotationRange);
            Vector3 endRotation;

            swordPivot.transform.localScale = new Vector3(1, SlashStartingScale, 1);
            swordPivot.transform.localRotation = Quaternion.Euler(0, RightSideMaxRotation,
                90 - ShoulderBaseRotation + startRotation);
            endRotation = new Vector3(0, LeftSideMaxRotation, 90 - ShoulderBaseRotation - startRotation);

            // have to wait 1 frame for the target position and rotation constraints to update
            yield return null;

            Anticipation();

            void Anticipation()
            {
                currentSwordPositionTween = DOVirtual.Vector3(currentSwordPosition, swordAttackTarget.transform.localPosition,
                    SlashAnticipationTime, ChangeSwordPosition).SetEase(Ease.InOutCubic);
                currentSwordRotationTween = swordHandTarget.DOLocalRotateQuaternion(swordAttackTarget.transform.localRotation,
                    SlashAnticipationTime).SetEase(Ease.InOutCubic);

                // intervals in sequence because it can be killed
                swordAttackSequence = DOTween.Sequence();
                swordAttackSequence.AppendInterval(SlashAnticipationTime);
                swordAttackSequence.AppendCallback(SlashBegins).AppendInterval(SlashOccurTime).AppendCallback(StartRecovery);
            }

            void SlashBegins()
            {
                swordTrail.Play();
                CurrentlySlashing = true;
                currentSwordPositionTween = swordPivot.transform.DOLocalRotate(endRotation, SlashOccurTime).SetEase(Ease.InOutCubic);
                currentSwordRotationTween = swordPivot.transform.DOScaleY(SlashEndingScale, SlashOccurTime / 2);
            }

            void StartRecovery()
            {
                StartCoroutine(Recovery());
            }

            IEnumerator Recovery()
            {
                // to make sure that the arm reaches the end on low framerates
                swordPivot.transform.localRotation = Quaternion.Euler(endRotation);
                swordPivot.transform.localScale = Vector3.one * SlashEndingScale;
                yield return null;
                currentSwordPosition = swordAttackTarget.localPosition;
                currentSwordRotation = swordAttackTarget.localRotation;

                swordTrail.Stop();
                recovering = true;
                currentSwordPositionTween = DOVirtual.Vector3(currentSwordPosition, defaultSwordPosition, SlashRecoveryTime,
                    ChangeSwordPosition).SetEase(Ease.InOutCubic).OnComplete(End);
                currentSwordRotationTween = swordHandTarget.DOLocalRotateQuaternion(defaultSwordRotation, SlashRecoveryTime).SetEase(Ease.InOutCubic);
                CurrentlySlashing = false;

                void End()
                {
                    currentSwordRotation = defaultSwordRotation;
                    currentlyInSlashState = false;
                    recovering = false;
                }
            }
        }
    }

    private void CancelSlash()
    {
        swordTrail.Stop();
        currentSwordPositionTween.Kill();
        currentSwordRotationTween.Kill();
        swordAttackSequence.Kill();
        currentSwordPosition = swordHandTarget.localPosition;
        currentSwordRotation = swordHandTarget.localRotation;
        currentlyInSlashState = false;
        CurrentlySlashing = false;
        recovering = false;
    }

    private void ChangeSwordPosition(Vector3 handPosition)
    {
        currentSwordPosition = handPosition;
    }
}
