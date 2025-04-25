using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MenuManager : MonoBehaviour
{
    private static readonly float InitialDelay = 0.1f, TransitionTime = 0.25f, CameraRotationSpeed = 10f;

    [SerializeField]
    private Image transitionImage;

    [SerializeField]
    private GameObject cameraObject;

    private bool transitioning;

    private void Start()
    {
        SaveData.Load();
        transitioning = false;

        transitionImage.color = new Color(0, 0, 0, 1);
        transitionImage.DOFade(0, TransitionTime).SetDelay(InitialDelay);
    }

    private void Update()
    {
        cameraObject.transform.Rotate(Vector3.up, CameraRotationSpeed * Time.deltaTime);
    }

    public void PressPlay()
    {
        if (transitioning)
            return;

        transitioning = true;
        transitionImage.DOFade(1, TransitionTime).OnComplete(() =>
        {
            if (!SaveData.FinalHubUnlocked)
                SceneManager.LoadScene("Hub");
            else
                SceneManager.LoadScene("FinalHub");
        });
    }

    public void PressQuit()
    {
        if (transitioning)
            return;

        transitioning = true;
        transitionImage.DOFade(1, TransitionTime).SetDelay(InitialDelay).OnComplete(() =>
        {
            Application.Quit();
        });
    }
}
