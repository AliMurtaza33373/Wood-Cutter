using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenEffectsUI : MonoBehaviour
{
    private readonly float GetHitStartAlpha = 1f, RemoveHitAlphaTime = 0.35f;

    [SerializeField]
    private Image hitImage;


    private void Start()
    {
        hitImage.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameplayEvents.OnPlayerGotHit += ShowHit;
    }

    private void OnDisable()
    {
        GameplayEvents.OnPlayerGotHit -= ShowHit;
    }

    private void ShowHit(float damage, Vector3 location)
    {
        hitImage.gameObject.SetActive(true);
        hitImage.color = new Color(1, 0, 0, GetHitStartAlpha);
        hitImage.DOColor(new Color(1, 0, 0, 0), RemoveHitAlphaTime).OnComplete(SetInactive);
        void SetInactive()
        {
            hitImage.gameObject.SetActive(false);
        }
    }
}
