using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public float bronzeTime;
    public float silverTime;
    public float goldTime;
    public float legendaryTime;
}
