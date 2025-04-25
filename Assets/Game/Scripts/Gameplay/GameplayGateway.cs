using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayGateway : MonoBehaviour
{
    [SerializeField]
    private LevelData levelData;

    [SerializeField]
    private GameObject block, finishCanvas, tutorialCanvas;

    [SerializeField]
    private TextMeshProUGUI timeText, highScoreText, legendaryTime, goldTime, silverTime, bronzeTime, energyGainedText, energyCollectedText, tutorialTextObject;

    [SerializeField]
    private GameObject[] medals, ticks;

    [SerializeField]
    private bool levelHasTutorial;

    [SerializeField]
    private string tutorialText;

    private int medalAquired;


    private void OnEnable()
    {
        GameplayEvents.OnPlayerPressLeftMouse += PlayerClosedTutorial;
    }

    private void OnDisable()
    {
        GameplayEvents.OnPlayerPressLeftMouse -= PlayerClosedTutorial;
    }

    private void Start()
    {
        if (levelHasTutorial && PlayerPrefs.GetInt(levelData.levelNumber.ToString() + "TutorialShown", 0) != 1)
        {
            PlayerPrefs.SetInt(levelData.levelNumber.ToString() + "TutorialShown", 1);
            tutorialCanvas.SetActive(true);
            tutorialTextObject.text = tutorialText;
        }
        else
        {
            tutorialCanvas.SetActive(false);
        }

        finishCanvas.SetActive(false);
        block.SetActive(true);
    }

    private void PlayerClosedTutorial()
    {
        if (tutorialCanvas.activeInHierarchy)
        {
            tutorialCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (!block.activeInHierarchy)
            return;

        if (GameManager.Instance.TreesRemaining <= 0)
            block.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.PlayerFinished();
            GameplaySoundManager.Instance.PlayWin();

            Cursor.lockState = CursorLockMode.None;
            finishCanvas.SetActive(true);

            for (int i = 0; i < ticks.Length; i++)
            {
                ticks[i].SetActive(false);
            }

            int extraEnergy;
            if (GameManager.Instance.CurrentTime <= levelData.legendaryTime)
            {
                extraEnergy = 12;
                medalAquired = 3;
            }
            else if (GameManager.Instance.CurrentTime <= levelData.goldTime)
            {
                extraEnergy = 12;
                medalAquired = 2;
            }
            else if (GameManager.Instance.CurrentTime <= levelData.silverTime)
            {
                extraEnergy = 8;
                medalAquired = 1;
            }
            else if (GameManager.Instance.CurrentTime <= levelData.bronzeTime)
            {
                extraEnergy = 4;
                medalAquired = 0;
            }
            else
            {
                extraEnergy = 0;
                medalAquired = -1;
            }

            if (medalAquired != -1)
                medals[medalAquired].SetActive(true);

            energyCollectedText.text = GameManager.Instance.TreesCut.ToString() + " Energy\nCollected";

            int totalEnergyGained;
            if (SaveData.Highscores[levelData.levelNumber - 1] == 0)
            {
                totalEnergyGained = GameManager.Instance.TreesCut + extraEnergy;
                energyGainedText.text = GameManager.Instance.TreesCut + " + " + extraEnergy + " Energy Gained";
            }
            else
            {
                energyCollectedText.text += "\n(Repeat)";

                int previousEnergyGained;
                if (SaveData.Highscores[levelData.levelNumber - 1] <= levelData.legendaryTime)
                {
                    previousEnergyGained = 12;
                    for (int i = 0; i < ticks.Length; i++)
                        ticks[i].SetActive(true);
                }
                else if (SaveData.Highscores[levelData.levelNumber - 1] <= levelData.goldTime)
                {
                    previousEnergyGained = 12;
                    for (int i = 0; i < ticks.Length - 1; i++)
                        ticks[i].SetActive(true);
                }
                else if (SaveData.Highscores[levelData.levelNumber - 1] <= levelData.silverTime)
                {
                    previousEnergyGained = 8;
                    for (int i = 0; i < ticks.Length - 2; i++)
                        ticks[i].SetActive(true);
                }
                else if (SaveData.Highscores[levelData.levelNumber - 1] <= levelData.bronzeTime)
                {
                    previousEnergyGained = 4;
                    for (int i = 0; i < ticks.Length - 3; i++)
                        ticks[i].SetActive(true);
                }
                else
                    previousEnergyGained = 0;

                totalEnergyGained = extraEnergy - previousEnergyGained;
                if (totalEnergyGained < 0)
                    totalEnergyGained = 0;

                energyGainedText.text = 0 + " + " + totalEnergyGained + " Energy Gained";
            }

            // save game
            SaveData.StoredEnergy += totalEnergyGained;
            if (SaveData.Highscores[levelData.levelNumber - 1] == 0 || SaveData.Highscores[levelData.levelNumber - 1] > GameManager.Instance.CurrentTime)
                SaveData.Highscores[levelData.levelNumber - 1] = GameManager.Instance.CurrentTime;
            if (SaveData.LevelsUnlocked <= levelData.levelNumber)
                SaveData.LevelsUnlocked = levelData.levelNumber + 1;
            SaveData.Save();

            // setup texts
            timeText.text = GameManager.Instance.CurrentTime.ToString("F1") + "s";
            highScoreText.text = SaveData.Highscores[levelData.levelNumber - 1].ToString("F1") + "s";
            bronzeTime.text = levelData.bronzeTime.ToString("F1") + "s";
            silverTime.text = levelData.silverTime.ToString("F1") + "s";
            goldTime.text = levelData.goldTime.ToString("F1") + "s";
            legendaryTime.text = levelData.legendaryTime.ToString("F1") + "s";
        }
    }

    public void RestartButtonPressed()
    {
        GameplayEvents.GameResetEvent();
    }

    public void ContinueButtonPressed()
    {
        GameManager.Instance.GoBackToHub();
    }
}
