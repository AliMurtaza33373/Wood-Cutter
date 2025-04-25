using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledParticleSystem : MonoBehaviour
{
    private ParticleSystem effect;

    private void Awake()
    {
        effect = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        effect.Play();
        DOVirtual.DelayedCall(effect.main.duration, () => { Lean.Pool.LeanPool.Despawn(gameObject); }, false);
    }
}
