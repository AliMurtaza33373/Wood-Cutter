using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySettings : MonoBehaviour
{
    [SerializeField]
    private Slider sensitivitySlider;

    [SerializeField]
    private TextMeshProUGUI sensitivityText;


    private void Start()
    {
        float sensitivityValue = PlayerPrefs.GetFloat("MouseSensitivity", 10f);
        sensitivityText.text = sensitivityValue.ToString();
        sensitivitySlider.value = sensitivityValue;
    }

    public void OnSensitivityChanged()
    {
        float sensitivityValue = sensitivitySlider.value;
        sensitivityText.text = sensitivityValue.ToString("F0");
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivityValue);
        UIEvents.SensitivityChangedEvent(sensitivityValue);
    }
}
