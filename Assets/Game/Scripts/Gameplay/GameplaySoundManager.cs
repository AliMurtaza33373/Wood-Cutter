using UnityEngine;

public class GameplaySoundManager : MonoBehaviour
{
    public static GameplaySoundManager Instance;


    private void Awake()
    {
        Instance = this;
    }
}
