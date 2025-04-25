using UnityEngine;

public static class GameplayEvents
{
    public delegate void CutTree(GameObject tree);
    public static event CutTree OnCutTree;
    public static void CutTreeEvent(GameObject tree)
    {
        if (OnCutTree != null) OnCutTree(tree);
    }

    public delegate void PlayerGotHit(float damage, Vector3 location);
    public static event PlayerGotHit OnPlayerGotHit;
    public static void PlayerGotHitEvent(float damage, Vector3 location)
    {
        if (OnPlayerGotHit != null) OnPlayerGotHit(damage, location);
    }

    public delegate void PlayerDied();
    public static event PlayerDied OnPlayerDied;
    public static void PlayerDiedEvent()
    {
        if (OnPlayerDied != null) OnPlayerDied();
    }

    public delegate void GameReset();
    public static event GameReset OnGameReset;
    public static void GameResetEvent()
    {
        if (OnGameReset != null) OnGameReset();
    }

    public delegate void PlayerPressLeftMouse();
    public static event PlayerPressLeftMouse OnPlayerPressLeftMouse;
    public static void PlayerPressLeftMouseEvent()
    {
        if (OnPlayerPressLeftMouse != null) OnPlayerPressLeftMouse();
    }
}
