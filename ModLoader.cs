using SALT.Config;
using SALT.Extensions;
using SALT.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SALT
{
    public class ModManager : USingleton<ModManager>
    {
        void Update() => Main.Update();

        void FixedUpdate() => Main.FixedUpdate();

        void LateUpdate() => Main.LateUpdate();

        void OnApplicationQuit()
        {
            Main.UnLoad();
            Debug.Log("Application ending after " + Time.time + " seconds");
        }

        SaveData saveData => MainScript.saveData;
        Dictionary<string, LevelSaveData> levelData
        {
            get
            {
                Dictionary<string, LevelSaveData> levelData = new Dictionary<string, LevelSaveData>();
                foreach (var kvp in saveData.levelData)
                    levelData.Add(kvp.Key, new LevelSaveData(kvp.Value.levelName, kvp.Value.stacheQuota, kvp.Value.allBubbas, kvp.Value.zeroDeaths, kvp.Value.caseClosed, kvp.Value.bestTime));
                return levelData;
            }
        }
        Dictionary<string, Dictionary<string, object>> levelSaveData
        {
            get
            {
                Dictionary<string, Dictionary<string, object>> levelData = new Dictionary<string, Dictionary<string, object>>();
                foreach (var kvp in saveData.levelData)
                    levelData.Add(kvp.Key, new Dictionary<string, object> { { "levelName", kvp.Value.levelName }, { "stacheQuota", kvp.Value.stacheQuota }, { "allBubbas", kvp.Value.allBubbas }, { "zeroDeaths", kvp.Value.zeroDeaths }, { "caseClosed", kvp.Value.caseClosed }, { "bestTime", kvp.Value.bestTime } });
                return levelData;
            }
        }
    }

    public enum LoadingStep
    {
        PRELOAD,
        LOAD,
        POSTLOAD,
        RELOAD,
        FINISHED,
        UNLOAD,
    }

    public class LoadingStepException : Exception
    {
        /// <summary>
        /// The loading step when the execption was created.
        /// </summary>
        public LoadingStep Step { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingStepException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes that error.</param>
        public LoadingStepException(string message) : base(message) => Step = ModLoader.CurrentLoadingStep;
    }

    public class ModNotFoundException : Exception
    {
        /// <summary>
        /// The id of the mod that was not found.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModNotFoundException"/> class with a mod id.
        /// </summary>
        /// <param name="modid">The id of the mod that was not found.</param>
        public ModNotFoundException(string modid) : base($"Cannot find '{modid}'") => ID = modid;
    }

    public static class ModLoader
    {
        internal const string ModJson = "modinfo.json";
        private static readonly Dictionary<string, Mod> Mods = new Dictionary<string, Mod>();
        private static readonly List<string> loadOrder = new List<string>();

        public static IEnumerable<ModInfo> LoadedMods => Mods.Select(x => x.Value.ModInfo);

        public static LoadingStep CurrentLoadingStep { get; private set; }

        internal static void InitializeMods()
        {
            FileSystem.CheckDirectory(FileSystem.ModPath);
            HashSet<ProtoMod> mods = new HashSet<ProtoMod>(new ProtoMod.Comparer());
            foreach (string file in Directory.GetFiles(FileSystem.ModPath, "modinfo.json", SearchOption.AllDirectories))
            {
                ProtoMod fromJson = ProtoMod.ParseFromJson(file);
                if (!mods.Add(fromJson))
                    throw new Exception("Found mod with duplicate id '" + fromJson.id + "' in " + file + "!");
            }
            foreach (string file in Directory.GetFiles(FileSystem.ModPath, "*.dll", SearchOption.AllDirectories))
            {
                ProtoMod mod;
                if (ProtoMod.TryParseFromDLL(file, out mod) && mod.id != null && !mods.Add(mod))
                    throw new Exception("Found mod with duplicate id '" + mod.id + "' in " + file + "!");
            }
            DependencyChecker.CheckDependencies(mods);
            DependencyChecker.CalculateLoadOrder(mods, loadOrder);
            DiscoverAndLoadAssemblies(mods);
        }

        public static bool IsModPresent(string modid) => loadOrder.Any(x => modid == x);
        public static ModInfo.ModVersion GetModVersion(string modid)
        {
            if (Mods.TryGetValue(modid, out Mod Mod))
                return Mod.ModInfo.Version;
            throw new ModNotFoundException(modid);
        }

        internal static bool TryGetEntryType(Assembly assembly, out Type entryType)
        {
            entryType = assembly.ManifestModule.GetTypes().FirstOrDefault(x => typeof(IModEntryPoint).IsAssignableFrom(x));
            return entryType != null;
        }

        private static void DiscoverAndLoadAssemblies(ICollection<ProtoMod> protomods)
        {
            HashSet<AssemblyInfo> foundAssemblies = new HashSet<AssemblyInfo>();
            foreach (ProtoMod protomod in protomods)
            {
                foreach (string file in Directory.GetFiles(protomod.path, "*.dll", SearchOption.AllDirectories))
                    foundAssemblies.Add(new AssemblyInfo(AssemblyName.GetAssemblyName(Path.GetFullPath(file)), Path.GetFullPath(file), protomod));
            }
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(FindAssembly);
            try
            {
                using (IEnumerator<ProtoMod> enumerator = protomods.GetEnumerator())
                {
                next:
                    if (enumerator.MoveNext())
                    {
                        ProtoMod mod = enumerator.Current;
                        foreach (AssemblyInfo assemblyInfo in foundAssemblies.Where(x => x.mod == mod))
                        {
                            Type entryType;
                            if (TryGetEntryType(assemblyInfo.LoadAssembly(), out entryType) && !assemblyInfo.IsModAssembly && (mod.isFromJSON || Path.GetFullPath(assemblyInfo.Path) == Path.GetFullPath(mod.entryFile)))
                            {
                                assemblyInfo.IsModAssembly = true;
                                AddMod(assemblyInfo.mod, entryType);
                                HarmonyOverrideHandler.LoadOverrides(entryType.Module);
                                goto next;
                            }
                        }
                        throw new EntryPointNotFoundException(string.Format("Could not find assembly for mod '{0}'", mod));
                    }
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(FindAssembly);
            }

            Assembly FindAssembly(object obj, ResolveEventArgs args)
            {
                AssemblyName name = new AssemblyName(args.Name);
                return foundAssemblies.FirstOrDefault(x => x.DoesMatch(name))?.LoadAssembly();
            }
        }

        internal static Mod GetMod(string id)
        {
            Mod Mod;
            return Mods.TryGetValue(id, out Mod) ? Mod : null;
        }

        internal static Mod GetModForAssembly(Assembly a) => Mods.FirstOrDefault(x => x.Value.EntryType.Assembly == a).Value;

        internal static ICollection<Mod> GetMods() => Mods.Values;

        internal static bool AllowSaves => GetMods().All(mod => mod.ModInfo.AllowSaves == true);

        private static Mod AddMod(ProtoMod modInfo, Type entryType)
        {
            try
            {
                IModEntryPoint entryPoint = (IModEntryPoint)Activator.CreateInstance(entryType);
                ModInfo info = modInfo.ToModInfo();
                Mod newMod = new Mod(info, entryPoint, modInfo.path);
                Mods.Add(modInfo.id, newMod);
                Console.Console.modsText += $"{(Console.Console.modsText.Equals(string.Empty) ? "" : "\n")}<color=#77DDFF>{info.Name}</color> [<color=#77DDFF>Author:</color> {info.Author} | <color=#77DDFF>ID:</color> {info.Id} | <color=#77DDFF>Version:</color> {info.Version}]";
                return newMod;
            }
            catch (Exception e)
            {
                throw new Exception($"Error adding mod '{modInfo.id}'!\n{e.GetType().Name}: {e.ParseTraceWithoutName()}");
            }
        }

        internal static void PreLoadMods()
        {
            Console.Console.Reload += Main.ReLoad;
            foreach (string key in loadOrder)
            {
                Mod mod = Mods[key];
                try
                {
                    ConfigManager.PopulateConfigs(mod);
                    mod.PreLoad();
                    //Console.Console.Reload += (Console.Console.ReloadAction)(() =>
                    //{
                    //    Mod.ForceModContext(mod);
                    //    foreach (ConfigFile config in mod.Configs)
                    //        config.TryLoadFromFile();
                    //    Mod.ClearModContext();
                    //});
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error pre-loading mod '{0}'!\n{1}: {2}", key, ex.GetType().Name, ex.ParseTraceWithoutName()));
                }
            }
        }

        internal static void LoadMods()
        {
            CurrentLoadingStep = LoadingStep.LOAD;
            foreach (string key in loadOrder)
            {
                Mod mod = Mods[key];
                try
                {
                    mod.Load();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error loading mod '{0}'!"+Environment.NewLine+"{1}: {2}", key, ex.GetType().Name, ex.ParseTraceWithoutName()));
                }
            }
        }

        internal static void PostLoadMods()
        {
            CurrentLoadingStep = LoadingStep.POSTLOAD;
            foreach (string key in loadOrder)
            {
                Mod mod = Mods[key];
                try
                {
                    mod.PostLoad();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error post-loading mod '{0}'!\n{1}: {2}", key, ex.GetType().Name, ex.ParseTraceWithoutName()));
                }
            }
            CurrentLoadingStep = LoadingStep.FINISHED;
        }

        internal static void ReLoadMods()
        {
            CurrentLoadingStep = LoadingStep.RELOAD;
            foreach (string key in loadOrder)
            {
                Mod mod = Mods[key];
                try
                {
                    Mod.ForceModContext(mod);
                    foreach (ConfigFile config in mod.Configs)
                        config.TryLoadFromFile();
                    Mod.ClearModContext();
                    mod.ReLoad();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error reloading mod '{0}'!\n{1}: {2}", key, ex.GetType().Name, ex.ParseTraceWithoutName()));
                }
            }
            CurrentLoadingStep = LoadingStep.FINISHED;
        }

        internal static void UnLoadMods()
        {
            CurrentLoadingStep = LoadingStep.UNLOAD;
            foreach (string key in loadOrder.ToList().Reverse<string>())
            {
                Mod mod = Mods[key];
                try
                {
                    mod.UnLoad();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error unloading mod '{0}'!\n{1}: {2}", key, ex.GetType().Name, ex.ParseTraceWithoutName()));
                }
            }
        }
        
        internal static void UpdateMods()
        {
            if (CurrentLoadingStep != LoadingStep.FINISHED) return;
            foreach (string key in loadOrder)
            {
                Mod mod = Mods[key];
                try
                {
                    mod.Update();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error updating mod '{0}'!\n{1}: {2}", key, ex.GetType().Name, ex.ParseTraceWithoutName()));
                }
            }
        }

        internal static void UpdateModsFixed()
        {
            if (CurrentLoadingStep != LoadingStep.FINISHED) return;
            foreach (string key in loadOrder)
            {
                Mod mod = Mods[key];
                try
                {
                    mod.FixedUpdate();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error fixed updating mod '{0}'!\n{1}: {2}", key, ex.GetType().Name, ex.ParseTraceWithoutName()));
                }
            }
        }

        internal static void UpdateModsLate()
        {
            if (CurrentLoadingStep != LoadingStep.FINISHED) return;
            foreach (string key in loadOrder)
            {
                Mod mod = Mods[key];
                try
                {
                    mod.LateUpdate();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error late updating mod '{0}'!\n{1}: {2}", key, ex.GetType().Name, ex.ParseTraceWithoutName()));
                }
            }
        }

        internal class AssemblyInfo
        {
            public AssemblyName AssemblyName;
            public string Path;
            public ProtoMod mod;
            public bool IsModAssembly;

            public AssemblyInfo(AssemblyName name, string path, ProtoMod mod)
            {
                this.AssemblyName = name;
                this.Path = path;
                this.mod = mod;
            }

            public bool DoesMatch(AssemblyName name) => name.Name == this.AssemblyName.Name;

            public Assembly LoadAssembly() => Assembly.LoadFrom(this.Path);
        }

        internal class ProtoInfo
        {
            public string id;
            public string name;
            public string author;
            public string version;
            public string description;
            public bool nosave;
            public string[] dependencies;
            public string[] load_after;
            public string[] load_before;

            public static implicit operator ProtoInfo(ProtoMod mod)
            {
                ProtoInfo info = new ProtoInfo();
                info.id = mod.id;
                info.name = mod.name;
                info.author = mod.author;
                info.version = mod.version;
                info.description = mod.description;
                info.nosave = mod.nosave;
                info.dependencies = mod.dependencies;
                info.load_after = mod.load_after;
                info.load_before = mod.load_before;
                return info;
            }

            public static implicit operator ProtoMod(ProtoInfo info)
            {
                ProtoMod mod = new ProtoMod();
                mod.id = info.id;
                mod.name = info.name;
                mod.author = info.author;
                mod.version = info.version;
                mod.description = info.description;
                mod.nosave = info.nosave;
                mod.dependencies = info.dependencies;
                mod.load_after = info.load_after;
                mod.load_before = info.load_before;
                return mod;
            }
        }

        internal class ProtoMod
        {
#pragma warning disable 0649
            public string id;
            public string name;
            public string author;
            public string version;
            public string description;
            public bool nosave;
            public string path;
            public string[] dependencies;
            public string[] load_after;
            public string[] load_before;
            public bool isFromJSON = true;
            public string entryFile;
#pragma warning restore 0649

            public override bool Equals(object o) => !(o is ProtoMod protoMod) ? base.Equals(o) : this.id == protoMod.id;

            public bool HasDependencies => this.dependencies != null && (uint)this.dependencies.Length > 0U;

            public static ProtoMod JsonToProto(string jsonData) => UnityEngine.JsonUtility.FromJson<ProtoInfo>(jsonData);

            public static ProtoMod ParseFromJson(string jsonFile) => ProtoMod.ParseFromJson(File.ReadAllText(jsonFile), jsonFile);

            public static ProtoMod ParseFromJson(string jsonData, string path)
            {
                ProtoMod protoMod = JsonToProto(jsonData);
                protoMod.path = Path.GetDirectoryName(path);
                protoMod.entryFile = path;
                protoMod.ValidateFields();
                return protoMod;
            }

            public static bool TryParseFromDLL(string dllFile, out ProtoMod mod)
            {
                Assembly assembly = Assembly.LoadFile(dllFile);
                mod = new ProtoMod();
                mod.isFromJSON = false;
                mod.path = Path.GetDirectoryName(dllFile);
                mod.entryFile = dllFile;
                string name = ((IEnumerable<string>)assembly.GetManifestResourceNames()).FirstOrDefault<string>((Func<string, bool>)(x => x.EndsWith("modinfo.json")));
                if (name == null)
                    return false;
                using (StreamReader streamReader = new StreamReader(assembly.GetManifestResourceStream(name)))
                {
                    mod = ParseFromJson(streamReader.ReadToEnd(), dllFile);
                    mod.isFromJSON = false;
                }
                return true;
            }

            public override string ToString() => this.id + " " + this.version;

            /// <summary>
            /// Make sure fields are in the correct form and not null
            /// </summary>
            private void ValidateFields()
            {
                this.id = this.id != null ? this.id.ToLower() : throw new Exception(this.path + " is missing an id field!");
                if (this.id.Contains(" "))
                    throw new Exception("Invalid mod id: " + this.id);
                this.load_after = this.load_after ?? new string[0];
                this.load_before = this.load_before ?? new string[0];
            }

            public ModInfo ToModInfo() => new ModInfo(this.id, this.name, this.author, ModInfo.ModVersion.Parse(this.version), this.description, this.nosave);

            public override int GetHashCode() => 1877310944 + EqualityComparer<string>.Default.GetHashCode(this.id);

            public class Comparer : IEqualityComparer<ProtoMod>
            {
                public bool Equals(ProtoMod x, ProtoMod y) => x.Equals(y);

                public int GetHashCode(ProtoMod obj) => obj.GetHashCode();
            }
        }
    }
}
