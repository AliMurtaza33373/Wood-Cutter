using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeTarget : MonoBehaviour
{
    private static readonly float RotationSpeed = 180f;

    [SerializeField]
    private GameObject availableObject, collectedObject, swingBlades, collectVFXPrefab;

    [HideInInspector]
    public bool collected;

    private void Start()
    {
        GameManager.Instance.AddTree();

        collected = false;
        availableObject.SetActive(true);
        collectedObject.SetActive(false);
    }

    private void Update()
    {
        if (collected)
            return;

        swingBlades.transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        GameplayRegistrations.RegisteredRopeTargets.Add(this);
    }

    private void OnDisable()
    {
        GameplayRegistrations.RegisteredRopeTargets.Remove(this);
    }

    public void CollectObject()
    {
        if (collected)
            return;

        GameplayEvents.CutTreeEvent(gameObject);

        collected = true;
        availableObject.SetActive(false);
        collectedObject.SetActive(true);
        Lean.Pool.LeanPool.Spawn(collectVFXPrefab, transform.position, Quaternion.identity);
    }
}
