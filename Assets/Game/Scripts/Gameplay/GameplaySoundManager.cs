using DG.Tweening;
using UnityEngine;

public class GameplaySoundManager : MonoBehaviour
{
    public static GameplaySoundManager Instance;


    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        HubEvents.OnPlayerStep += RunOnConcrete;
    }

    private void OnDisable()
    {
        HubEvents.OnPlayerStep -= RunOnConcrete;
    }

    [SerializeField]
    private AudioClip woshClip, slashTreeClip, gainEnergyClip, destroyClip, getHitClip, dieClip, winClip;

    [SerializeField]
    private AudioClip[] runConcreteClips, jumpClips, landClips;


    private AudioSource audioSource;


    public void PlayWin()
    {
        audioSource.PlayOneShot(winClip);
    }

    public void PlayDie()
    {
        audioSource.PlayOneShot(dieClip);
    }

    public void PlayGetHit()
    {
        audioSource.PlayOneShot(getHitClip);
    }

    public void PlayDestroy()
    {
        audioSource.PlayOneShot(destroyClip);
    }

    public void PlayGainEnergy()
    {
        audioSource.PlayOneShot(gainEnergyClip);
    }

    public void PlaySlashTree()
    {
        audioSource.PlayOneShot(slashTreeClip);
    }

    public void PlayWosh()
    {
        audioSource.PlayOneShot(woshClip);
    }

    public void RunOnConcrete()
    {
        PlayRandomClip(runConcreteClips);
    }

    public void PlayJump()
    {
        PlayRandomClip(jumpClips);
    }

    public void PlayLand()
    {
        PlayRandomClip(landClips);
    }

    private void PlayRandomClip(AudioClip[] walkClips)
    {
        int randomIndex = UnityEngine.Random.Range(0, walkClips.Length);
        audioSource.PlayOneShot(walkClips[randomIndex]);
    }
}
