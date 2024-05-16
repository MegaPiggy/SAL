using Enum = System.Enum;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using SALT;
using SALT.Utils;
using SALT.Extensions;

public static class Levels
{
    public const string MAIN_MENU = "LevelSelect";
    private static Scene replacedScene;
    private static Scene newScene;
    public static Scene LastScene => replacedScene;
    public static Scene CurrentScene => newScene;

    static Levels()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    
    }

    private static void OnActiveSceneChanged(Scene replaced, Scene next)
    {
        SALT.Console.Console.Log("Active Scene Changed");
        replacedScene = replaced;
        newScene = next;
        Callbacks.OnActiveSceneChanged_Trigger(replaced.name.FromSceneName(), next.name.FromSceneName());
    }

    internal static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SALT.Console.Console.Log("Scene Loaded");
        Callbacks.OnSceneLoaded();
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        SALT.Console.Console.Log("Scene Unloaded");
        Callbacks.OnSceneUnloaded();
    }

    public static readonly Level DONT_DESTROY_ON_LOAD = (Level)Enum.ToObject(typeof(Level), -1);
    
    public static Level CurrentLevel
    {
        get
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene == null) return DONT_DESTROY_ON_LOAD;
            return scene.name.FromSceneName();
        }
    }

    public static string LevelName => LevelLoader.levelName;

    public static bool isMainMenu() => IsLevel(MAIN_MENU) || IsLevel(Level.MAIN_MENU);

    public static bool isTitleScreen() => isMainMenu() && MainScript.title;

    public static bool IsLevel(string name) => SceneManager.GetActiveScene().name == name;

    public static bool IsLevel(int index) => SceneManager.GetActiveScene().buildIndex == index;

    public static bool IsLevel(Level level) => CurrentLevel == level;

    public static bool isOffice() => IsLevel(Level.OFFICE);//=> SceneManager.GetActiveScene().buildIndex == 1;
    public static bool isPopOnRocks() => IsLevel(Level.POP_ON_ROCKS);//=> SceneManager.GetActiveScene().buildIndex == 2;
    public static bool isRedHeart() => IsLevel(Level.RED_HEART);// => SceneManager.GetActiveScene().buildIndex == 3;
    public static bool isPekoland() => IsLevel(Level.PEKO_LAND);//SceneManager.GetActiveScene().buildIndex == 4;
    public static bool isOfficeReversed() => IsLevel(Level.OFFICE_REVERSED);//SceneManager.GetActiveScene().buildIndex == 5;
    public static bool isToTheMoon() => IsLevel(Level.TO_THE_MOON);//SceneManager.GetActiveScene().buildIndex == 6;
    public static bool isNothing() => IsLevel(Level.NOTHING);//SceneManager.GetActiveScene().buildIndex == 7;
    public static bool isMoguMogu() => IsLevel(Level.MOGU_MOGU);//SceneManager.GetActiveScene().buildIndex == 8;
    public static bool isInumore() => IsLevel(Level.INUMORE);//SceneManager.GetActiveScene().buildIndex == 9;
    public static bool isRushia() => IsLevel(Level.RUSHIA);//SceneManager.GetActiveScene().buildIndex == 10;
    public static bool isInascapableMadness() => IsLevel(Level.INASCAPABLE_MADNESS);//SceneManager.GetActiveScene().buildIndex == 11;
    public static bool isHereComesHope() => IsLevel(Level.HERE_COMES_HOPE);//SceneManager.GetActiveScene().buildIndex == 12;
    public static bool isReflect() => IsLevel(Level.REFLECT);//SceneManager.GetActiveScene().buildIndex == 13;
}