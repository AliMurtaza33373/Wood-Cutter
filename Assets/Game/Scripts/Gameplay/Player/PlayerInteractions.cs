using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    public static readonly float MAX_CANVAS_DISTANCE_X = 300f, MAX_CANVAS_DISTANCE_Y = 175f, MAX_DISTANCE_ROPE = 20f;
    private readonly float CheckMaxAngle = 90f, SlashStartDelay = 0.01f;

    public RaycastHit CurrentRopeTarget { get; private set; }
    public bool IsRopeAttached { get { return playerGadgets.IsRopeAttached; } }
    public float CurrentPlayerEnergy { get { return player.CurrentEnergy; } }
    public bool RopeTargetAvailable { get; set; }


    public static PlayerInteractions Instance { get; private set; }


    [SerializeField]
    private GameObject ropeTargetImage, root;
    [SerializeField]
    private RectTransform canvasRect;

    private Player player;
    private PlayerGadgets playerGadgets;
    private PlayerArms playerArms;
    private PlayerMovement playerMovement;
    private Camera mainCam;


    private List<GameObject> treesInSlashRange = new List<GameObject>();

    private RopeTarget currentRopeTargetScript;
    private float timeRemainSlash;

    private void Awake()
    {
        if (Instance != null)
            Debug.LogError("Two player prefabs found");
        Instance = this;
        player = GetComponent<Player>();
        playerGadgets = GetComponent<PlayerGadgets>();
        playerArms = GetComponent<PlayerArms>();
        playerMovement = GetComponent<PlayerMovement>();
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        GameplayEvents.OnCutTree += TreeCut;
    }

    private void OnDisable()
    {
        GameplayEvents.OnCutTree -= TreeCut;
    }

    private void Update()
    {
        CurrentRopeTarget = GetCurrentRopeTarget();

        if (CurrentRopeTarget.point == Vector3.zero ||
            Mathf.Abs(ropeTargetImage.transform.localPosition.x) > (canvasRect.rect.width / 2) - MAX_CANVAS_DISTANCE_X ||
            Mathf.Abs(ropeTargetImage.transform.localPosition.y) > (canvasRect.rect.height / 2) - MAX_CANVAS_DISTANCE_Y)
            RopeTargetAvailable = false;
        else
            RopeTargetAvailable = true;
    }

    private void FixedUpdate()
    {
        if (playerArms.CurrentlySlashing)
            SlashEverythingInRange();
        else
            timeRemainSlash = SlashStartDelay;
    }

    private void SlashEverythingInRange()
    {
        if (timeRemainSlash > 0)
        {
            timeRemainSlash -= Time.deltaTime;
            return;
        }

        if (treesInSlashRange.Count > 0)
        {
            for (int i = 0; i < treesInSlashRange.Count; i++)
            {
                if (treesInSlashRange[i] != null)
                    treesInSlashRange[i].GetComponent<MechanicalTree>().CutDownTree();
            }
        }

        treesInSlashRange.Clear();
    }

    private RaycastHit GetCurrentRopeTarget()
    {
        RaycastHit returnHit = new RaycastHit();

        if (playerGadgets.IsRopeAttached)
        {
            if (GameplayRegistrations.RegisteredRopeTargets.Contains(currentRopeTargetScript))
            {
                Vector3 direction = currentRopeTargetScript.transform.position - mainCam.transform.position;
                if (direction.sqrMagnitude < MAX_DISTANCE_ROPE * MAX_DISTANCE_ROPE)
                {
                    if (Physics.Raycast(mainCam.transform.position + (direction.normalized * 0.5f), direction, out RaycastHit hitInfo, Mathf.Infinity))
                    {
                        if (hitInfo.point != Vector3.zero)
                            returnHit = hitInfo;
                        else
                            returnHit = CurrentRopeTarget;
                    }
                    else
                        returnHit = CurrentRopeTarget;
                }
            }
        }
        else if (GameplayRegistrations.RegisteredRopeTargets.Count > 0)
        {
            float currentLowestAngle = 90;
            foreach (RopeTarget target in GameplayRegistrations.RegisteredRopeTargets)
            {
                Vector3 direction = target.transform.position - mainCam.transform.position;
                if (Physics.Raycast(mainCam.transform.position + (direction.normalized * 0.5f), direction, out RaycastHit hitInfo, Mathf.Infinity))
                {
                    bool correctTarget = (hitInfo.collider.GetComponent<RopeTarget>() != null) || (hitInfo.collider.GetComponentInChildren<RopeTarget>() != null);
                    if (correctTarget && Vector3.Distance(mainCam.transform.position, target.transform.position) < MAX_DISTANCE_ROPE)
                    {
                        Vector3 directionToHit = target.transform.position - mainCam.transform.position;
                        float angleToHit = Vector3.Angle(mainCam.transform.forward, directionToHit);
                        if (angleToHit < CheckMaxAngle && angleToHit < currentLowestAngle)
                        {
                            currentLowestAngle = angleToHit;
                            returnHit = hitInfo;
                            currentRopeTargetScript = target;
                        }
                    }
                }
            }
        }

        return returnHit;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Tree") && !treesInSlashRange.Contains(other.gameObject))
            treesInSlashRange.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tree") && treesInSlashRange.Contains(other.gameObject))
            treesInSlashRange.Remove(other.gameObject);
    }

    private void TreeCut(GameObject tree)
    {
        if (treesInSlashRange.Contains(tree))
            treesInSlashRange.Remove(tree);
    }

    public void LandedOnFlower()
    {
        playerMovement.FlowerExplodeUp();
    }
}
