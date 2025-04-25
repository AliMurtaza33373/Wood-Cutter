public static class UIEvents
{
    public delegate void SensitivityChanged(float newSensitivity);
    public static event SensitivityChanged OnSensitivityChanged;
    public static void SensitivityChangedEvent(float newSensitivity)
    {
        if (OnSensitivityChanged != null) OnSensitivityChanged(newSensitivity);
    }
}
