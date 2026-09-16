using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "el_mandoob_save.json";

    public static void Save(GameData data)
    {
        if (data == null)
        {
            data = new GameData();
        }

        try
        {
            Normalize(data);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetPath(), json);
        }
        catch (Exception exception)
        {
            Debug.LogError("El Mandoob: save failed. " + exception.Message);
        }
    }

    public static GameData Load()
    {
        string path = GetPath();

        if (File.Exists(path))
        {
            try
            {
                GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(path));
                if (data != null)
                {
                    Normalize(data);
                    return data;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("El Mandoob: save was unreadable, starting a safe new save. " + exception.Message);
            }
        }

        GameData fresh = new GameData();
        Save(fresh);
        return fresh;
    }

    private static void Normalize(GameData data)
    {
        if (data.stars == null || data.stars.Length != 8)
        {
            int[] oldStars = data.stars;
            data.stars = new int[8];
            if (oldStars != null)
            {
                Array.Copy(oldStars, data.stars, Mathf.Min(oldStars.Length, data.stars.Length));
            }
        }

        for (int i = 0; i < data.stars.Length; i++)
        {
            data.stars[i] = Mathf.Clamp(data.stars[i], 0, 3);
        }

        data.healths = Mathf.Clamp(data.healths <= 0 ? 1 : data.healths, 1, 3);
        data.speed = Mathf.Clamp(data.speed <= 0 ? 5 : data.speed, 5, 8);
        data.audios = data.audios == 0 ? 0 : 1;
        data.money = Mathf.Max(0, data.money);
        data.reputation = Mathf.Max(0, data.reputation);
        data.completedDeliveries = Mathf.Max(0, data.completedDeliveries);
        data.completedShifts = Mathf.Max(0, data.completedShifts);
        data.totalTips = Mathf.Max(0, data.totalTips);
        data.storyStage = Mathf.Clamp(data.storyStage, 0, 6);
        data.levelUnlocked = Mathf.Clamp(data.levelUnlocked <= 0 ? 1 : data.levelUnlocked, 1, 8);
    }

    private static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }
}
