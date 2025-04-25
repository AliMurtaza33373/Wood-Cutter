using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeSlashEffect : MonoBehaviour
{
    [SerializeField]
    private Transform swordTransformReference, pointsReference;

    private ParticleSystem slashParticleSystem;
    private ParticleSystem.Particle[] slashParticles;
    private float[] slashParticlesYPositions;

    private void Awake()
    {
        slashParticleSystem = GetComponent<ParticleSystem>();

        slashParticles = new ParticleSystem.Particle[slashParticleSystem.main.maxParticles];
        slashParticlesYPositions = new float[slashParticles.Length];
    }

    private void LateUpdate()
    {
        int numParticlesAlive = slashParticleSystem.GetParticles(slashParticles);
        pointsReference.rotation = swordTransformReference.localRotation;
        for (int i = 0; i < numParticlesAlive; i++)
        {
            if (slashParticles[i].remainingLifetime >= slashParticles[i].startLifetime - Time.deltaTime)
                slashParticlesYPositions[i] = (slashParticles[i].position - swordTransformReference.localPosition).magnitude;
            else
                slashParticles[i].position = swordTransformReference.localPosition + (pointsReference.up * slashParticlesYPositions[i]);
        }
        slashParticleSystem.SetParticles(slashParticles, numParticlesAlive);
    }
}
