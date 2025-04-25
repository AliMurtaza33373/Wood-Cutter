using DG.Tweening;
using UnityEngine;

public class GameplaySoundManager : MonoBehaviour
{
    public static GameplaySoundManager Instance;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        GameplayEvents.OnPlayerStep += RunOnConcrete;
    }

    private void OnDisable()
    {
        GameplayEvents.OnPlayerStep -= RunOnConcrete;
    }

    [SerializeField]
    private AudioClip woshClip, slashTreeClip, gainEnergyClip, destroyClip, getHitClip, dieClip, winClip;

    [SerializeField]
    private AudioClip[] runConcreteClips, jumpClips, landClips;


    private AudioSource audioSource;


    public void PlayWin()
    {
        if (Instance == null)
            return;
        audioSource.PlayOneShot(winClip);
    }

    public void PlayDie()
    {
        if (Instance == null)
            return;
        audioSource.PlayOneShot(dieClip);
    }

    public void PlayGetHit()
    {
        if (Instance == null)
            return;

        audioSource.PlayOneShot(getHitClip);
    }

    public void PlayDestroy()
    {
        if (Instance == null)
            return;
        audioSource.PlayOneShot(destroyClip);
    }

    public void PlayGainEnergy()
    {
        if (Instance == null)
            return;
        audioSource.PlayOneShot(gainEnergyClip);
    }

    public void PlaySlashTree()
    {
        if (Instance == null)
            return;
        audioSource.PlayOneShot(slashTreeClip);
    }

    public void PlayWosh()
    {
        if (Instance == null)
            return;
        audioSource.PlayOneShot(woshClip);
    }

    public void RunOnConcrete()
    {
        if (Instance == null)
            return;
        PlayRandomClip(runConcreteClips);
    }

    public void PlayJump()
    {
        if (Instance == null)
            return;
        PlayRandomClip(jumpClips);
    }

    public void PlayLand()
    {
        if (Instance == null)
            return;
        PlayRandomClip(landClips);
    }

    private void PlayRandomClip(AudioClip[] walkClips)
    {
        int randomIndex = UnityEngine.Random.Range(0, walkClips.Length);
        audioSource.PlayOneShot(walkClips[randomIndex]);
    }
}
