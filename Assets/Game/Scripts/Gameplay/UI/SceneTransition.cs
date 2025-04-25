using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    private static readonly float TransitionDuration = 0.25f;

    [SerializeField]
    private Image darkImage;

    private bool resetting;

    private void OnEnable()
    {
        GameplayEvents.OnGameReset += StartTransition;
    }

    private void OnDisable()
    {
        GameplayEvents.OnGameReset -= StartTransition;
    }

    private void Start()
    {
        resetting = false;
        darkImage.gameObject.SetActive(true);
        darkImage.color = new Color(0, 0, 0, 1);
        darkImage.DOFade(0, TransitionDuration).OnComplete(() =>
        {
            darkImage.gameObject.SetActive(false);
        }); 
    }

    private void StartTransition()
    {
        if (resetting)
            return;

        resetting = true;
        
        if (GameManager.Instance.IsGameOver)
            GameManager.Instance.RestartScene();
        else
        {
            darkImage.gameObject.SetActive(true);
            darkImage.color = new Color(0, 0, 0, 0);
            darkImage.DOFade(1, TransitionDuration).OnComplete(() =>
            {
                GameManager.Instance.RestartScene();
            });
        }
    }
}
