using TMPro;
using UnityEngine;

public class MessagesUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI remainingText, timeText, gatewayText;

    private void Start()
    {
        gatewayText.gameObject.SetActive(false);
    }


    private void Update()
    {
        if (GameManager.Instance.TreesRemaining > 0)
            remainingText.text = GameManager.Instance.TreesRemaining.ToString() + " More Energy Needed";
        else
        {
            remainingText.gameObject.SetActive(false);
            gatewayText.gameObject.SetActive(true);
        }

        timeText.text = GameManager.Instance.CurrentTime.ToString("F1") + "s";
    }
}
