using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FinalSceneChangeUI : MonoBehaviour
{
    private static readonly float InitialLoadDelay = 0.1f, BackgroundShowTime = 0.25f, AfterButtonPressDelay = 1f, WhitenTime = 5f;

    [SerializeField]
    private Image backgroundImage, whiteBackgroundImage;


    private void OnEnable()
    {
        HubEvents.OnWentInsideGateway += ShowBackground;

    }

    private void OnDisable()
    {
        HubEvents.OnWentInsideGateway -= ShowBackground;
    }

    private void Start()
    {
        if (!SaveData.FinalHubUnlocked)
        {
            SaveData.FinalHubUnlocked = true;
            SaveData.Save();

            whiteBackgroundImage.gameObject.SetActive(true);
            whiteBackgroundImage.color = new Color(1, 1, 1, 1);
            backgroundImage.gameObject.SetActive(false);

            DOVirtual.DelayedCall(AfterButtonPressDelay, StartShowingNow);

            void StartShowingNow()
            {
                whiteBackgroundImage.DOFade(0, WhitenTime).OnComplete(DisableBackground);
            }

            void DisableBackground()
            {
                whiteBackgroundImage.gameObject.SetActive(false);
            }
        }
        else
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
}
