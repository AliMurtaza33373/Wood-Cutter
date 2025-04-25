using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SceneChangeUI : MonoBehaviour
{
    private static readonly float InitialLoadDelay = 0.1f, BackgroundShowTime = 0.25f, AfterButtonPressDelay = 3f, WhitenTime = 7f;

    [SerializeField]
    private Image backgroundImage, whiteBackgroundImage;


    private void OnEnable()
    {
        HubEvents.OnWentInsideGateway += ShowBackground;
        HubEvents.OnPressedButton += ShowWhite;

    }

    private void OnDisable()
    {
        HubEvents.OnWentInsideGateway -= ShowBackground;
        HubEvents.OnPressedButton -= ShowWhite;
    }

    private void Start()
    {
        backgroundImage.gameObject.SetActive(true);
        backgroundImage.color = new Color(0, 0, 0, 1);
        backgroundImage.DOFade(0, BackgroundShowTime + InitialLoadDelay).OnComplete(DisableBackground);
        whiteBackgroundImage.gameObject.SetActive(false);

        void DisableBackground()
        {
            backgroundImage.gameObject.SetActive(false);
        }
    }


    private void ShowBackground(string sceneName)
    {
        backgroundImage.gameObject.SetActive(true);
        backgroundImage.color = new Color(0, 0, 0, 0);
        backgroundImage.DOColor(new Color(0, 0, 0, 1), BackgroundShowTime).OnComplete(LoadSceneNow);

        void LoadSceneNow()
        {
            HubManager.Instance.LoadScene(sceneName);
        }
    }

    private void ShowWhite()
    {
        DOVirtual.DelayedCall(AfterButtonPressDelay, StartWhiteningNow);

        void StartWhiteningNow()
        {
            whiteBackgroundImage.gameObject.SetActive(true);
            whiteBackgroundImage.color = new Color(1, 1, 1, 0);
            whiteBackgroundImage.DOFade(1, WhitenTime);
        }
    }
}
