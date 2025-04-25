using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static readonly float MAX_ENERGY = 3f;
    public float CurrentEnergy { get; private set; }


    private void OnEnable()
    {
        GameplayEvents.OnPlayerGotHit += GotHit;
    }

    private void OnDisable()
    {
        GameplayEvents.OnPlayerGotHit -= GotHit;
    }

    private void Start()
    {
        CurrentEnergy = MAX_ENERGY;
    }

    private void GotHit(float damage, Vector3 location)
    {
        if (CurrentEnergy > damage)
            CurrentEnergy -= damage;
        else
        {
            GameplayEvents.PlayerDiedEvent();
        }
    }
}

