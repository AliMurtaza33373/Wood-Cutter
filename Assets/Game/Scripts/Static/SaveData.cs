using UnityEngine;
using System.IO;
using System;

[Serializable]
public static class SaveData
{
    private static readonly int MaxLevels = 16;

    public static int StoredEnergy { get; set; }
    public static int LevelsUnlocked { get; set; }
    public static float[] Highscores { get; set; } = new float[MaxLevels];
    public static bool FinalHubUnlocked { get; set; }


    [Serializable]
    private class SaveItems
    {
        public int savedStoredEnergy, savedLevelsUnlocked;
        public float[] savedHighscores = new float[MaxLevels];
        public bool savedFinalHubUnlocked;
    }


    public static void Save()
    {
        SaveItems saveItems = new SaveItems();

        saveItems.savedStoredEnergy = StoredEnergy;
        saveItems.savedLevelsUnlocked = LevelsUnlocked;
        for (int i = 0; i < MaxLevels; i++)
        {
            saveItems.savedHighscores[i] = Highscores[i];
        }
        saveItems.savedFinalHubUnlocked = FinalHubUnlocked;

        string json = JsonUtility.ToJson(saveItems);

        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }

    public static void Load()
    {
        string path = Application.persistentDataPath + "/savefile.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveItems data = JsonUtility.FromJson<SaveItems>(json);

            StoredEnergy = data.savedStoredEnergy;
            LevelsUnlocked = data.savedLevelsUnlocked;
            for (int i = 0; i < MaxLevels; i++)
            {
                Highscores[i] = data.savedHighscores[i];
            }
            FinalHubUnlocked = data.savedFinalHubUnlocked;
        }
        else
        {
            StoredEnergy = 0;
            LevelsUnlocked = 1;
            for (int i = 0; i < MaxLevels; i++)
            {
                Highscores[i] = 0;
            }
            FinalHubUnlocked = false;
        }
    }

    public static void LoadCustom(int loadStoredEnergy, int loadLevelsUnlocked, float loadHighscores, bool finalHubUnlocked)
    {
        StoredEnergy = loadStoredEnergy;
        LevelsUnlocked = loadLevelsUnlocked;
        for (int i = 0; i < MaxLevels; i++)
            Highscores[i] = loadHighscores;
        FinalHubUnlocked = finalHubUnlocked;
    }
}
