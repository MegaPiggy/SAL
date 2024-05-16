using SALT.Utils;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SALT
{
    public static class FileSystem
    {
        public const string DataPath = "Smol Ame_Data";
        public const string ModPath = "SALT/Mods";
        public const string LibPath = "SALT/Libs";

        public static string CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public static string GetMyPath()
        {
            Assembly relevantAssembly = ReflectionUtils.GetRelevantAssembly();
            return ModLoader.GetModForAssembly(relevantAssembly)?.Path ?? Path.GetDirectoryName(relevantAssembly.Location);
        }

        internal static string GetConfigPath(Mod mod) => CheckDirectory(Path.Combine(Path.Combine(Application.persistentDataPath, "SALT/Config"), mod?.ModInfo.Id ?? "SALT"));

        public static string GetMyConfigPath() => GetConfigPath(Mod.GetCurrentMod());
    }
}
