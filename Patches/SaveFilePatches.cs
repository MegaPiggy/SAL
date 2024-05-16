using SALT;
using SALT.Console;
using SALT.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using HarmonyLib;

public static class SaveScript
{
    internal static bool stopSave = false;
    private static Dictionary<string, LevelSaveData> levelData = new Dictionary<string, LevelSaveData>();
    private static SaveData saveData
    {
        get => MainScript.saveData;
        set => MainScript.saveData = value;
    }
    private static MainScript main
    {
        get => MainScript.main;
        set => MainScript.main = value;
    }
    private static bool title
    {
        get => MainScript.title;
        set => MainScript.title = value;
    }
    private static Vector3 spawnPoint
    {
        get => MainScript.spawnPoint;
        set => MainScript.spawnPoint = value;
    }

    public static void SaveSaveData()
    {
        try
        {
            bool deleting = ReflectionUtils.FromMethod("DeleteSaveData");
            bool canSave = deleting || (ModLoader.AllowSaves && !stopSave);
            if (!canSave)
            {
                Dictionary<string, LevelSaveData> levelData = new Dictionary<string, LevelSaveData>();
                foreach (var kvp in SaveScript.levelData)
                    levelData.Add(kvp.Key, new LevelSaveData(kvp.Value.levelName, kvp.Value.stacheQuota, kvp.Value.allBubbas, kvp.Value.zeroDeaths, kvp.Value.caseClosed, kvp.Value.bestTime));
                saveData.levelData = levelData;
                Console.LogError("Save failed stop (" + !stopSave + " and " + ModLoader.AllowSaves + ") or " + deleting + " = " + canSave);
            }
            else
            {
                string text = Application.persistentDataPath + "/save.dat";
                FileStream fileStream = (!File.Exists(text)) ? File.Create(text) : File.OpenWrite(text);
                new BinaryFormatter().Serialize(fileStream, saveData);
                fileStream.Close();
                StoreSaveData();
                Console.LogSuccess("Game saved to " + text);
            }
        }
        catch (System.Exception ex)
        {
            Console.LogError("Save failed exception");
            Console.LogException(ex);
        }
    }

    public static void BackupSaveData()
    {
        try
        {
            string path = $"{Application.persistentDataPath}/saveBackup.dat";
            FileStream fileStream = !File.Exists(path) ? File.Create(path) : File.OpenWrite(path);
            if (ModLoader.AllowSaves)
                new BinaryFormatter().Serialize(fileStream, saveData);
            else
                new BinaryFormatter().Serialize(fileStream, new SaveData
                {
                    levelData = levelData,
                });
            fileStream.Close();
            Console.LogSuccess($"Game saved to {path}");
        }
        catch (System.Exception ex)
        {
            Console.LogError("Save failed exception backup");
            Console.LogException(ex);
        }
    }

    public static void LoadSaveData()
    {
        try
        {
            string path = Application.persistentDataPath + "/save.dat";
            if (File.Exists(path))
            {
                FileStream fileStream = File.OpenRead(path);
                saveData = (SaveData)new BinaryFormatter().Deserialize(fileStream);
                fileStream.Close();
                main.showGPtut = false;
                Console.LogSuccess("Game loaded");
            }
            else
            {
                Console.LogSuccess("New game created");
            }
        }
        catch (System.Exception ex)
        {
            Console.LogError("Load failed");
            Console.LogException(ex);
            SALT.UI.ErrorUI.CreateError("An error occurred while loading your save file!\n"+ex.ParseTrace());
        }
        StoreSaveData();
    }

    public static void DeleteSaveData()
    {
        try
        {
            BackupSaveData();
            levelData = new Dictionary<string, LevelSaveData>();
            saveData = new SaveData();
            SaveSaveData();
            title = true;
            spawnPoint = Vector3.zero;
            LevelLoader.loader.LoadLevel(0);
        }
        catch (System.Exception ex)
        {
            Console.LogError("Delete failed");
            Console.LogException(ex);
        }
    }

    private static void StoreSaveData()
    {
        try
        {
            levelData = new Dictionary<string, LevelSaveData>();
            foreach (var kvp in saveData.levelData)
                levelData.Add(kvp.Key, new LevelSaveData(kvp.Value.levelName, kvp.Value.stacheQuota, kvp.Value.allBubbas, kvp.Value.zeroDeaths, kvp.Value.caseClosed, kvp.Value.bestTime));
        }
        catch (System.Exception ex)
        {
            Console.LogError("Storage failed");
            Console.LogException(ex);
        }
    }
}

namespace SALT.Patches
{

    [HarmonyPatch(typeof(MainScript), nameof(MainScript.SaveSaveData))]
    internal static class SavePatch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix()
        {
            SaveScript.SaveSaveData();
            return false;
        }
    }

    [HarmonyPatch(typeof(MainScript), nameof(MainScript.BackupSaveData))]
    internal static class BackupSavePatch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix()
        {
            SaveScript.BackupSaveData();
            return false;
        }
    }

    [HarmonyPatch(typeof(MainScript), nameof(MainScript.LoadSaveData))]
    internal static class LoadSavePatch
    {
        [HarmonyPriority(Priority.First)]
        private static bool Prefix()
        {
            SaveScript.LoadSaveData();
            return false;
        }
    }

    [HarmonyPatch(typeof(MainScript), nameof(MainScript.DeleteSaveData))]
    internal static class DeleteSavePatch
    {
        [HarmonyPriority(Priority.First)]
        private static bool Prefix()
        {
            SaveScript.DeleteSaveData();
            return false;
        }
    }
}
