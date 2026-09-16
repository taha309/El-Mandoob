using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "el_mandoob_save.json";
    private const string LegacyFileName = "data.dvb";

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

        // One-time best-effort import from the original Delivery Boy binary save.
        GameData legacy = TryLoadLegacy();
        if (legacy != null)
        {
            Normalize(legacy);
            Save(legacy);
            return legacy;
        }

        GameData fresh = new GameData();
        Save(fresh);
        return fresh;
    }

    private static GameData TryLoadLegacy()
    {
        string legacyPath = Path.Combine(Application.persistentDataPath, LegacyFileName);
        if (!File.Exists(legacyPath))
        {
            return null;
        }

        try
        {
#pragma warning disable SYSLIB0011
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(legacyPath, FileMode.Open))
            {
                return formatter.Deserialize(stream) as GameData;
            }
#pragma warning restore SYSLIB0011
        }
        catch (Exception exception)
        {
            Debug.LogWarning("El Mandoob: legacy save could not be imported. " + exception.Message);
            return null;
        }
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
