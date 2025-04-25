using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private readonly float SlowAmount = 0.4f, KillSlowTime = 0.075f;

    private float defaultFixedDeltaTime;

    public static GameManager Instance { get; private set; }

    [SerializeField]
    private GameObject canvasObject;

    public int TreesRemaining { get; private set; }
    public int TreesCut { get; private set; }
    public float CurrentTime { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsGamePaused { get; private set; }


    private void Awake()
    {
        Instance = this;
        TreesRemaining = 0;
        TreesCut = 0;
        CurrentTime = 0;
        IsGameOver = false;
        IsGamePaused = false;
        canvasObject.gameObject.SetActive(false);
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        GameplayEvents.OnPlayerDied += RestartScene;
        GameplayEvents.OnCutTree += CutTree;
    }

    private void OnDisable()
    {
        GameplayEvents.OnPlayerDied -= RestartScene;
        GameplayEvents.OnCutTree -= CutTree;
    }

    private void Update()
    {
        if (IsGameOver)
            return;

        CurrentTime += Time.deltaTime;
    }

    public void RestartScene()
    {
        Time.timeScale = 1;
        DOTween.CompleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoBackToHub()
    {
        Time.timeScale = 1;
        DOTween.CompleteAll();
        if (!SaveData.FinalHubUnlocked)
            SceneManager.LoadScene("Hub");
        else
            SceneManager.LoadScene("FinalHub");
    }

    private void CutTree(GameObject tree)
    {
        TreesRemaining--;
        TreesCut++;

        Time.timeScale = SlowAmount;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
        DOVirtual.DelayedCall(KillSlowTime, BackToNormalSpeed, false).SetUpdate(true);

        void BackToNormalSpeed()
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
        }
    }

    public void PlayerFinished()
    {
        IsGameOver = true;
    }

    public void AddTree()
    {
        TreesRemaining++;
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
}
