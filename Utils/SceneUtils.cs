using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using SALT.Extensions;
using SALT.Registries;

namespace SALT.Utils
{
    public static class SceneUtils
    {
        public static int SceneLength => SceneManager.sceneCount;
        public static int SceneBuildLength => SceneManager.sceneCountInBuildSettings;
        public static Dictionary<int, string> SceneNames
        {
            get 
            {
                Dictionary<int, string> names = new Dictionary<int, string>();
                foreach (var normalScene in Main.sceneNames)
                    names.Add(normalScene.Key, normalScene.Value);
                foreach (var moddedScene in moddedSceneNames)
                    names.Add(moddedScene.Key, moddedScene.Value);
                return names;
            }
        }
        public static Scene GetScene(int index) => SceneManager.GetSceneAt(index);
        public static Scene GetSceneByLevel(Level level)
        {
            int index = (int)level;
            if (index == -1)
                return SceneUtils.GetDontDestroyOnLoadScene();
            else if (level.IsVanilla())
                return SceneManager.GetSceneByBuildIndex(index);
            else if (level.IsModded() && Registries.LevelRegistry.customScenes.ContainsKey(level))
                return Registries.LevelRegistry.customScenes[level];
            else
                throw new Exception($"No scene found for level '{level}'");
        }
        public static bool TryGetSceneByLevel(Level level, out Scene scene)
        {
            int index = (int)level;
            if (index == -1)
            {
                scene = SceneUtils.GetDontDestroyOnLoadScene();
                return true;
            }
            else if(level.IsVanilla())
            {
                scene = SceneManager.GetSceneByBuildIndex(index);
                return true;
            }
            else if (level.IsModded() && Registries.LevelRegistry.customScenes.ContainsKey(level))
            {
                scene = Registries.LevelRegistry.customScenes[level];
                return true;
            }
            else
            {
                scene = SceneUtils.GetDontDestroyOnLoadScene();
                return false;
            }
        }
        public static Scene GetSceneByBuildIndex(int index) => SceneManager.GetSceneByBuildIndex(index);
        public static string GetSceneName(int index) => GetScene(index).name;
        public static string GetSceneNameByBuildIndex(int index) => GetSceneByBuildIndex(index).name;
        public static Scene GetSceneFromName(string name) => SceneManager.GetSceneByName(name);
        public static Scene GetSceneFromPath(string path) => SceneManager.GetSceneByPath(path);
        public static string GetPathFromName(string name)
        {
            var regex = new Regex(@"([^/]*/)*([\w\d\-]*)\.unity");
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var gname = regex.Replace(path, "$2");
                if (gname == name)
                    return path;
            }
            return string.Empty;
        }
        public static Scene GetDontDestroyOnLoadScene()
        {
            GameObject temp = null;
            try
            {
                temp = new GameObject();
                UnityEngine.Object.DontDestroyOnLoad(temp);
                Scene dontDestroyOnLoad = temp.scene;
                UnityEngine.Object.DestroyImmediate(temp);
                temp = null;

                return dontDestroyOnLoad;
            }
            finally
            {
                if (temp != null)
                    UnityEngine.Object.DestroyImmediate(temp);
            }
        }
        public static Scene GetActiveScene() => SceneManager.GetActiveScene();
        public static bool SetActiveScene(Scene scene) => SceneManager.SetActiveScene(scene);
        private static LoadSceneParameters sceneParameters = new LoadSceneParameters { loadSceneMode = LoadSceneMode.Single, localPhysicsMode = LocalPhysicsMode.Physics2D };
        private static LoadSceneParameters sceneParametersSpecial = new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics2D };
        public static Scene LoadScene(Scene scene) => SceneManager.LoadScene(scene.name, sceneParameters);
        public static Scene LoadScene(string name) => SceneManager.LoadScene(name, sceneParameters);
        public static Scene LoadScene(int index) => SceneManager.LoadScene(index, sceneParameters);
        public static Scene LoadSceneSpecial(int index) => SceneManager.LoadScene(index, sceneParametersSpecial);
        public static void LoadSceneAsync(Scene scene, Action<AsyncOperation> onComplete) => SceneManager.LoadSceneAsync(scene.name, sceneParameters).completed += onComplete;
        public static void LoadSceneAsync(string name, Action<AsyncOperation> onComplete) => SceneManager.LoadSceneAsync(name, sceneParameters).completed += onComplete;
        public static void LoadSceneAsync(int index, Action<AsyncOperation> onComplete) => SceneManager.LoadSceneAsync(index, sceneParameters).completed += onComplete;
        public static void LoadSceneAsyncSpecial(int index, Action<AsyncOperation> onComplete) => SceneManager.LoadSceneAsync(index, sceneParametersSpecial).completed += onComplete;
        public static string GetScenePathByBuildIndex(int index) => SceneUtility.GetScenePathByBuildIndex(index);
        public static Scene GetBuildScene(int i) => GetSceneFromPath(SceneUtility.GetScenePathByBuildIndex(i));

        private static void QuickAsync(AsyncOperation operation, int buildIndex, Action<Scene> func)
        {
            operation.allowSceneActivation = true;
            Scene loaded = GetBuildScene(buildIndex);
            func(loaded);
            Debug.Log("Done Before");
            SceneManager.UnloadSceneAsync(loaded);
        }

        public static void QuickLoad(int buildIndex, Action<Scene> func) => LoadSceneAsyncSpecial(buildIndex, a => QuickAsync(a, buildIndex, func));

        public static int GetBuildIndexByScenePath(string path) => SceneUtility.GetBuildIndexByScenePath(path);
        public static Scene GetBuildSceneByName(string name) => SceneUtils.GetAllBuildScenes().FirstOrDefault(s => s.name == name);
        public static PhysicsScene2D GetPhysicsScene2D(Scene scene) => scene.GetPhysicsScene2D();
        public static PhysicsScene GetPhysicsScene(Scene scene) => scene.GetPhysicsScene();
        public static void MoveGameObjectToScene(GameObject go, Scene scene) => SceneManager.MoveGameObjectToScene(go, scene);

        internal static void CheckDifference() => Console.Console.Log("Length = " + SceneLength + " | BuildLength = " + SceneBuildLength);

        public static List<Scene> GetAllScenes()
        {
            var numScenes = SceneLength;
            List<Scene> scenes = new List<Scene>(numScenes);
            for (int i = 0; i < numScenes; ++i)
                scenes.Add(GetScene(i));
            return scenes;
        }

        public static List<string> GetAllSceneNames()
        {
            var numScenes = SceneLength;
            List<string> sceneNames = new List<string>(numScenes);

            for (int i = 0; i < numScenes; ++i)
                sceneNames.Add(GetSceneName(i));
            return sceneNames;
        }

        public static List<Scene> GetAllBuildScenes()
        {
            var numScenes = SceneBuildLength;
            List<Scene> scenes = new List<Scene>(numScenes);
            for (int i = 0; i < numScenes; ++i)
                scenes.Add(GetSceneByBuildIndex(i));
            return scenes;
        }

        public static List<Scene> GetAllBuildScenesByPath()
        {
            List<Scene> scenes = new List<Scene>();
            var regex = new Regex(@"([^/]*/)*([\w\d\-]*)\.unity");
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                scenes.Add(GetSceneFromPath(SceneUtility.GetScenePathByBuildIndex(i)));
            return scenes;
        }

        public static List<string> GetAllBuildSceneNames()
        {
            var numScenes = SceneBuildLength;
            List<string> sceneNames = new List<string>(numScenes);

            for (int i = 0; i < numScenes; ++i)
                sceneNames.Add(GetSceneNameByBuildIndex(i));
            return sceneNames;
        }

        public static void UnloadBuildScenes()
        {
            foreach (Scene scene in GetAllBuildScenes())
            {
                if (scene.isLoaded)
                    scene.Unload();
            }
        }

        private static List<Scene> moddedScenes = new List<Scene>();

        internal static Dictionary<int, string> moddedSceneNames = new Dictionary<int, string>();
        private static int moddedSceneNum = 0;
        internal static List<Scene> ModdedScenes => moddedScenes.ToList();

        internal static void UnloadModdedScene(Scene scene)
        {
            scene.Unload();
            //foreach (GameObject root in scene.GetRootGameObjects())
            //    root.DestroyImmediate();
        }

        public static void UnloadModdedScene(Level level)
        {
            if (Registries.LevelRegistry.customScenes.ContainsKey(level))
                UnloadModdedScene(Registries.LevelRegistry.customScenes[level]);
        }

        public static void UnloadAllModdedScene()
        {
            foreach (Scene moddedScene in ModdedScenes)
                UnloadModdedScene(moddedScene);
        }

        public static void LoadModdedScene(Level level)
        {
            Scene scene = Registries.LevelRegistry.CreateLevel(level);

            LevelLoader.level = (int)level;
            LevelLoader.levelName = level.ToSceneName();
            HUDScript.HUD.StartLoad();

            //MainScript.loading = true;
            MusicLooper.looper.fadeOut = true;

            GameObject mainLevelStuff = scene.InstantiateInactive(SAObjects.MainLevelStuff);
            LevelManager levelManager = mainLevelStuff.GetComponent<LevelManager>();
            if (LevelRegistry.moustacheGoals.ContainsKey(level))
            {
                Pair<int, float> goal = LevelRegistry.moustacheGoals[level];
                levelManager.totalMoustaches = goal.First;
                levelManager.moustacheQuota = goal.Second;
                levelManager.moustacheQuotaInt = Mathf.RoundToInt(goal.First * goal.Second);
            }
            else
            {
                levelManager.totalMoustaches = 0;
                levelManager.moustacheQuota = 0;
                levelManager.moustacheQuotaInt = 0;
            }
            GameObject player = scene.InstantiateInactive(SAObjects.Player);
            PlayerScript playerScript = player.GetComponent<PlayerScript>();
            playerScript.camTarget = mainLevelStuff.FindChild("CamTarget", true).transform;
            playerScript.camLookTarget = mainLevelStuff.FindChild("CamLookTarget", true).transform;
            playerScript.pointer = mainLevelStuff.FindChild("Pointer", true).transform;
            playerScript.rotEmpty = mainLevelStuff.FindChild("RotEmpty", true).transform;
            playerScript.checkpoint = mainLevelStuff.FindChild("Checkpoint", true).transform;
            playerScript.groundNormalEmpty = mainLevelStuff.FindChild("GroundNormalEmpty", true).transform;
            playerScript.canRestart = true;

            Registries.LevelRegistry.InvokeSceneCreationEvent(level, scene, mainLevelStuff);

            //SceneUtils.LoadScene(scene);
            SceneUtils.SetActiveScene(scene);
            SceneUtils.UnloadBuildScenes();

            mainLevelStuff.SetActive(true);
            player.SetActive(true);
            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                obj.SetActive(true);
            }

            HUDScript.HUD.isLoading = false;
            HUDScript.HUD.transitionStart = false;
            if (LevelRegistry.skyboxes.ContainsKey(level))
                RenderSettings.skybox = LevelRegistry.skyboxes[level].Instantiate();
            else
                RenderSettings.skybox = SAObjects.Skybox.Instantiate();
            playerScript.currentState = PlayerState.Moving;
            playerScript.SnapCam();

            LevelLoader.level = -1;
            MainScript.loading = false;
            LevelLoader.loadCo = null;

            Levels.OnSceneLoaded(scene, LoadSceneMode.Single);
        }

        internal static Scene CreateScene(string name)
        {
            Scene newScene = SceneManager.CreateScene(name);
            moddedSceneNum--;
            moddedScenes.Add(newScene);
            moddedSceneNames.Add(moddedSceneNum, name);
            return newScene;
        }
        internal static Scene CreateScene(string name, CreateSceneParameters parameters)
        {
            Scene newScene = SceneManager.CreateScene(name, parameters);
            moddedSceneNum--;
            moddedScenes.Add(newScene);
            moddedSceneNames.Add(moddedSceneNum, name);
            return newScene;
        }
    }
}
