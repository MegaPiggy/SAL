using System;
using System.Collections.Generic;
using System.Linq;
using SALT.Extensions;
using SALT.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SALT.Registries
{
    public static class LevelRegistry
    {
        public delegate void ModdedSceneLoad(Scene scene, GameObject mainLevelStuff);

        internal static IDRegistry<Level> moddedIds = new IDRegistry<Level>();

        internal static Dictionary<Level, Scene> customScenes = new Dictionary<Level, Scene>();

        internal static Dictionary<Level, ModdedSceneLoad> customSceneEvents = new Dictionary<Level, ModdedSceneLoad>();

        internal static Dictionary<Level, Material> skyboxes = new Dictionary<Level, Material>();

        internal static Dictionary<Level, Pair<int, float>> moustacheGoals = new Dictionary<Level, Pair<int, float>>();

        static LevelRegistry()
        {
            ModdedIDRegistry.RegisterIDRegistry(moddedIds);
        }

        public static Level CreateLevelId(string name)
        {
            if (ModLoader.CurrentLoadingStep > LoadingStep.PRELOAD)
                throw new LoadingStepException("Can't register identifiables outside of the PreLoad step");
            object value = EnumPatcher.GetFirstFreeValue<Level>(-1);
            return moddedIds.RegisterValueWithEnum((Level)value, name.ToUpper().Replace(" ", "_"));
        }

        public static Level CreateLevelId(object value, string name)
        {
            if (ModLoader.CurrentLoadingStep > LoadingStep.PRELOAD)
                throw new LoadingStepException("Can't register identifiables outside of the PreLoad step");
            if (Convert.ToInt32(value) == -1)
                throw new EnumPatcherException(typeof(Level), $"Cannot add enum value of -1 to enum type \"{typeof(Level).FullName}\"");
            return moddedIds.RegisterValueWithEnum((Level)value, name.ToUpper().Replace(" ", "_"));
        }

        public static void RegisterSceneCreationEvent(Level level, ModdedSceneLoad @event)
        {
            customSceneEvents.Add(level, @event);
        }

        public static void RegisterSkyboxMaterial(Level level, Material skybox)
        {
            skyboxes.Add(level, skybox);
        }

        /// <summary>
        /// Register the total amount of moustaches in the level, and the percentage the player needs to collect.
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="totalMoustaches">Total number of moustaches in the level</param>
        /// <param name="moustacheQuota">Percentage of moustaches the player needs to collect</param>
        public static void RegisterMoustacheGoal(Level level, int totalMoustaches, float moustacheQuota)
        {
            moustacheGoals.Add(level, new Pair<int, float>(totalMoustaches, moustacheQuota));
        }

        internal static void InvokeSceneCreationEvent(Level level, Scene scene, GameObject mainLevelStuff)
        {
            if (customSceneEvents.ContainsKey(level))
                customSceneEvents[level].Invoke(scene, mainLevelStuff);
        }

        internal static Scene CreateLevel(Level level) => CreateLevel(level, LocalPhysicsMode.None);//LocalPhysicsMode.None

        internal static Scene CreateLevel(Level level, LocalPhysicsMode localPhysicsMode)
        {
            if (level.IsVanilla())
            {
                Console.Console.LogError("Trying to create a scene for a vanilla level!");
                return SceneManager.GetSceneByBuildIndex((int)level);
            }
            else if (customScenes.ContainsKey(level))
            {
                Console.Console.LogError("Trying to create a scene for a level that already has one!");
                return customScenes[level];
            }
            Scene scene = SceneUtils.CreateScene(level.ToSceneName(), new CreateSceneParameters(localPhysicsMode));
            customScenes.Add(level, scene);
            SceneUtils.CheckDifference();
            return scene;
        }
    }
}
