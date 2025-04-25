using TMPro;
using UnityEngine;

public class Gateway : MonoBehaviour
{
    private static readonly float DownDistance = 5f, MedalRotationSpeed = 60f;

    [SerializeField]
    private LevelData levelData;

    [SerializeField]
    private GameObject[] medals;

    [SerializeField]
    private TextMeshProUGUI highScoreText, levelNumberText;

    [SerializeField]
    private GameObject canvasObject;

    private GameObject currentMedal;

    private void Start()
    {
        levelNumberText.text = levelData.levelNumber.ToString();

        if (levelData.levelNumber > SaveData.LevelsUnlocked)
        {
            canvasObject.SetActive(false);
            transform.Translate(Vector3.down * DownDistance);
        }
        else if (SaveData.Highscores[levelData.levelNumber - 1] != 0)
        {
            canvasObject.SetActive(true);
            highScoreText.text = SaveData.Highscores[levelData.levelNumber - 1].ToString("F1") + "s";

            if (SaveData.Highscores[levelData.levelNumber - 1] <= levelData.legendaryTime)
                currentMedal = medals[3];
            else if (SaveData.Highscores[levelData.levelNumber - 1] <= levelData.goldTime)
                currentMedal = medals[2];
            else if (SaveData.Highscores[levelData.levelNumber - 1] <= levelData.silverTime)
                currentMedal = medals[1];
            else if (SaveData.Highscores[levelData.levelNumber - 1] <= levelData.bronzeTime)
                currentMedal = medals[0];

            if (currentMedal != null)
            {
                currentMedal.SetActive(true);
                currentMedal.transform.Rotate(Vector3.back * Random.Range(0, 360), Space.Self);
            }
        }
    }

    private void Update()
    {
        if (currentMedal != null)
        {
            currentMedal.transform.Rotate(Vector3.back * Time.deltaTime * MedalRotationSpeed, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (SaveData.LevelsUnlocked >= levelData.levelNumber && other.CompareTag("Player"))
            HubEvents.WentInsideGatewayEvent(levelData.levelNumber.ToString());
    }
}
