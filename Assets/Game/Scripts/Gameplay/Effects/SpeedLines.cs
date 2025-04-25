using UnityEngine;

public class SpeedLines : MonoBehaviour
{
    private readonly float MinRunSpeed = 8f, MaxRunSpeed = 19;
    private readonly float MinRateOverTime = 50f, MaxRateOverTime = 300f;
    private readonly float MinAlpha = 0f, MaxAlpha = 1f;

    private ParticleSystem speedLines;
    private ParticleSystem.EmissionModule emmisionModule;
    ParticleSystem.MainModule mainModule;

    private Color currentColor;

    [SerializeField]
    private PlayerMovement playerMovement;

    private void Awake()
    {
        speedLines = GetComponent<ParticleSystem>();
        emmisionModule = speedLines.emission;
        mainModule = speedLines.main;
        currentColor = Color.white;
    }

    private void Update()
    {
        Vector3 horizontalVelocity = playerMovement.CurrentPlayerVelocity;
        horizontalVelocity.y = 0;

        if (horizontalVelocity.sqrMagnitude > MinRunSpeed * MinRunSpeed)
        {
            if (!speedLines.isPlaying)
                speedLines.Play();

            float lerpAmount = (horizontalVelocity.magnitude - MinRunSpeed) / (MaxRunSpeed - MinRunSpeed);
            emmisionModule.rateOverTime = Mathf.Lerp(MinRateOverTime, MaxRateOverTime, lerpAmount);
            currentColor.a = Mathf.Lerp(MinAlpha, MaxAlpha, lerpAmount);
            mainModule.startColor = currentColor;
        }
        else
            speedLines.Stop();
    }
}
