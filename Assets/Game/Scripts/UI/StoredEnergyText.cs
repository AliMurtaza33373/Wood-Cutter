using TMPro;
using UnityEngine;

public class StoredEnergyText : MonoBehaviour
{
    [SerializeField]
    private GameObject otherText;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        HubEvents.OnPressedButton += RemoveText;
    }

    private void OnDisable()
    {
        HubEvents.OnPressedButton -= RemoveText;
    }

    private void Start()
    {
        text.text = SaveData.StoredEnergy.ToString();
    }

    private void RemoveText()
    {
        gameObject.SetActive(false);
        otherText.SetActive(false);
    }
}
