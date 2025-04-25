public static class HubEvents
{
    public delegate void WentInsideGateway(string sceneName);
    public static event WentInsideGateway OnWentInsideGateway;
    public static void WentInsideGatewayEvent(string sceneName)
    {
        if (OnWentInsideGateway != null) OnWentInsideGateway(sceneName);
    }

    public delegate void PressedButton();
    public static event PressedButton OnPressedButton;
    public static void PressedButtonEvent()
    {
        if (OnPressedButton != null) OnPressedButton();
    }

    public delegate void PlayerStep();
    public static event PlayerStep OnPlayerStep;
    public static void PlayerStepEvent()
    {
        if (OnPlayerStep != null) OnPlayerStep();
    }
}
