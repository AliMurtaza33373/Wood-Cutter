using DG.Tweening;
using System;
using UnityEngine;

public class HubSoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip pressButtonClip, hubTransformationClip;

    [SerializeField]
    private AudioClip[] walkDirtClips, walkConcreteClips;

    public static HubSoundManager Instance;

    private AudioSource audioSource;

    private float volume = 1f;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayPressButtonSound()
    {
        audioSource.PlayOneShot(pressButtonClip);
    }

    public void PlayHubTransformationSound()
    {
        DOVirtual.Float(1, 0, 4f, ChangeVolume);

        audioSource.PlayOneShot(hubTransformationClip);
    }

    public void WalkOnDirt()
    {
        PlayRandomClip(walkDirtClips);
    }

    public void WalkOnConcrete()
    {
        PlayRandomClip(walkConcreteClips);
    }

    private void PlayRandomClip(AudioClip[] walkClips)
    {
        int randomIndex = UnityEngine.Random.Range(0, walkClips.Length);
        audioSource.PlayOneShot(walkClips[randomIndex]);
    }

    private void ChangeVolume(float value)
    {
        volume = value;
    }

    public void FirstTimeFinalHub()
    {
        DOVirtual.Float(0, 1, 2f, ChangeVolume);
    }
}
