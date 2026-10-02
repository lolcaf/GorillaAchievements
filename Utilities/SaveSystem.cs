using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GorillaAchievements.Utilities;

internal static class SaveSystem
{
    public static Save userSave = new Save();

    private static string SaveDirectory => Path.Combine(Application.persistentDataPath, Constants.Name);

    private static string SavePath => Path.Combine(SaveDirectory, "UnlockedAchievements.json");

    public static void SaveData()
    {
        try
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            string json = JsonUtility.ToJson(userSave, false);
            File.WriteAllText(SavePath, json);
            Plugin.Log.WriteLine("Saved data");
        }
        catch (Exception ex)
        {
            Plugin.Log.WriteLine($"Failed to save data: {ex}");
        }
    }

    public static void LoadData()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                userSave = new Save();
                return;
            }

            string json = File.ReadAllText(SavePath);
            userSave = JsonUtility.FromJson<Save>(json) ?? new Save();

            Plugin.Log.WriteLine("Loaded Data");
        }
        catch (Exception ex)
        {
            Plugin.Log.WriteLine($"Failed to load data: {ex}");
            userSave = new Save();
        }
    }

    [Serializable]
    public class Save
    {
        public List<string> unlockedAchievements = new List<string>();
    }
}
