using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RopeTargetUI : MonoBehaviour
{
    [SerializeField]
    private GameObject target;

    private void LateUpdate()
    {
        if (PlayerInteractions.Instance != null)
        {
            if (PlayerInteractions.Instance.IsRopeAttached || PlayerInteractions.Instance.CurrentRopeTarget.point == Vector3.zero)
                SetInactive();
            else
                ShowTarget();
        }
        else
            SetInactive();
    }

    private void ShowTarget()
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(PlayerInteractions.Instance.CurrentRopeTarget.point);

        if (PlayerInteractions.Instance.RopeTargetAvailable)
            target.SetActive(true);
        else
            target.SetActive(false);

        target.transform.position = screenPoint;
    }

    private void SetInactive()
    {
        target.transform.position = Vector3.zero;

        if (target.activeInHierarchy)
            target.SetActive(false);
    }
}
