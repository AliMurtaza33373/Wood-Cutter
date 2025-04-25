using DG.Tweening;
using UnityEngine;

public class Platform : MonoBehaviour
{
    private static readonly int RequiredEnergy = 150;

    [SerializeField]
    private GameObject requiredObject, pressEObject;

    private Animator animator;

    private bool canPressButton;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        requiredObject.SetActive(false);
        pressEObject.SetActive(false);
        canPressButton = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (SaveData.StoredEnergy >= RequiredEnergy)
            {
                canPressButton = true;
                pressEObject.SetActive(true);
            }
            else
                requiredObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            requiredObject.SetActive(false);
            pressEObject.SetActive(false);
            canPressButton = false;
        }
    }

    public void AttemptPressButton()
    {
        if (canPressButton)
        {
            HubEvents.PressedButtonEvent();
            HubSoundManager.Instance.PlayPressButtonSound();
            animator.SetTrigger("PressButton");

            DOVirtual.DelayedCall(4f, PlayTransformationSoundNow);

            void PlayTransformationSoundNow()
            {
                HubSoundManager.Instance.PlayHubTransformationSound();
            }
        }
    }

    public void LoadNextHub()
    {
        HubManager.Instance.LoadScene("FinalHub");
    }
}
