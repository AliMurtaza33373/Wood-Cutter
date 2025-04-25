using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class HubManager : MonoBehaviour
{
    public static HubManager Instance;

    [SerializeField]
    public Platform platform;

    [SerializeField]
    private GameObject canvasObject;

    public bool IsGamePaused { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        SaveData.Load();
    }

    private void Start()
    {
        IsGamePaused = false;
        canvasObject.gameObject.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void PauseUnPause()
    {
        if (!IsGamePaused)
        {
            Cursor.lockState = CursorLockMode.None;
            IsGamePaused = true;
            Time.timeScale = 0;
            canvasObject.gameObject.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            IsGamePaused = false;
            Time.timeScale = 1;
            canvasObject.gameObject.SetActive(false);
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1;
        DOTween.CompleteAll();
        SceneManager.LoadScene("MainMenu");
    }
}
