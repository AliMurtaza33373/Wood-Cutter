using UnityEngine;

public class PlayerGadgets : MonoBehaviour
{
    private readonly float RopeTravelSpeed = 100f;

    // rope animation properties
    private readonly float damper = 14f, strength = 800f, velocity = 15f, waveHeight = 1f;
    private readonly int quality = 150, waveCount = 3;


    public bool IsRopeAttached { get; private set; }
    public bool IsRopeThrown { get { return swingingRopeState == RopeState.Throwing || swingingRopeState == RopeState.Attached; } }


    [SerializeField]
    private GameObject root, ropeStartPoint, ropeEnd, ropeConnectPrefab;
    [SerializeField]
    private LineRenderer ropeRenderer;
    [SerializeField]
    private AnimationCurve ropeAnimationCurve;

    private PlayerCamera playerCamera;


    private Spring spring;
    private Vector3 ropeTargetPoint, attachPointNormal, ropeEndPoint;
    private float currentRopeDistance;

    private bool ropePressed, waitingFrame;

    private RopeState swingingRopeState;
    private enum RopeState
    {
        None,
        Throwing,
        Attached,
        Returning
    }

    private void Awake()
    {
        playerCamera = GetComponent<PlayerCamera>();
        spring = new Spring();
        spring.SetTarget(0);
    }

    private void Update()
    {
        RopeFunctionality();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void RopeFunctionality()
    {
        IsRopeAttached = false;

        if (swingingRopeState == RopeState.Throwing && PlayerInteractions.Instance.CurrentRopeTarget.point != Vector3.zero)
        {
            currentRopeDistance += RopeTravelSpeed * Time.deltaTime;
            if (currentRopeDistance >= Vector3.Distance(ropeStartPoint.transform.position, ropeTargetPoint))
            {
                Lean.Pool.LeanPool.Spawn(ropeConnectPrefab, ropeTargetPoint, Quaternion.identity);
                playerCamera.RopeAttachShake();
                swingingRopeState = RopeState.Attached;
            }
            waitingFrame = true;
        }
        else if (ropePressed || waitingFrame)
        {
            waitingFrame = false;

            if (swingingRopeState == RopeState.Attached && PlayerInteractions.Instance.CurrentRopeTarget.point != Vector3.zero)
            {
                RopeTarget ropeTarget = PlayerInteractions.Instance.CurrentRopeTarget.collider.gameObject.GetComponent<RopeTarget>();
                if (ropeTarget != null && !ropeTarget.collected)
                    ropeTarget.CollectObject();

                IsRopeAttached = true;
                currentRopeDistance = Vector3.Distance(ropeStartPoint.transform.position, ropeTargetPoint);
                ropeTargetPoint = PlayerInteractions.Instance.CurrentRopeTarget.point;
                attachPointNormal = PlayerInteractions.Instance.CurrentRopeTarget.normal;
            }
            else if (PlayerInteractions.Instance.RopeTargetAvailable)
            {
                swingingRopeState = RopeState.Throwing;
                currentRopeDistance = 0;
                spring.SetVelocity(velocity);
                ropeRenderer.positionCount = quality + 1;
                ropeTargetPoint = PlayerInteractions.Instance.CurrentRopeTarget.point;
                attachPointNormal = PlayerInteractions.Instance.CurrentRopeTarget.normal;
            }
            else if (!(swingingRopeState == RopeState.None))
                NoTarget();
        }
        else if (!(swingingRopeState == RopeState.None))
            NoTarget();

        void NoTarget()
        {
            if (swingingRopeState == RopeState.Attached || swingingRopeState == RopeState.Throwing)
                swingingRopeState = RopeState.Returning;

            currentRopeDistance -= RopeTravelSpeed * Time.deltaTime;
            if (currentRopeDistance <= 0)
                swingingRopeState = RopeState.None;
        }
    }

    public void SetRopePressed(bool pressed)
    {
        ropePressed = pressed;
    }

    private void DrawRope()
    {
        if (swingingRopeState == RopeState.None)
        {
            ropeRenderer.positionCount = 0;
            ropeEnd.SetActive(false);
            return;
        }

        ropeEnd.SetActive(true);
        Vector3 startPosition = ropeStartPoint.transform.position;
        if (swingingRopeState == RopeState.Attached)
        {
            ropeEndPoint = ropeTargetPoint;
            ropeEnd.transform.LookAt(ropeEnd.transform.position - attachPointNormal);
        }
        else
        {
            ropeEndPoint = startPosition + ((ropeTargetPoint - startPosition).normalized * currentRopeDistance);
            ropeEnd.transform.LookAt(ropeTargetPoint);
        }
        ropeEnd.transform.position = ropeEndPoint;

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 up = Vector3.zero;
        if (ropeEndPoint - startPosition != Vector3.zero)
            up = Quaternion.LookRotation((ropeEndPoint - startPosition).normalized) * Vector3.up;

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI)
                * spring.Value * ropeAnimationCurve.Evaluate(delta);

            ropeRenderer.SetPosition(quality - i, Vector3.Lerp(startPosition, ropeEndPoint, delta) + offset);
        }
    }
}
