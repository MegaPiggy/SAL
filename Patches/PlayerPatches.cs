using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using SALT.Extensions;
using SALT.Registries;
using SALT.Utils;

namespace SALT.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Awake")]
    internal static class PlayerAwakePatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(PlayerScript __instance)
        {
            Main.player = __instance.gameObject;
            Console.Console.Log("Current: " + Levels.CurrentLevel);
            if (Levels.CurrentLevel.IsModded())
                __instance.characterPacks = Registries.CharacterRegistry.GetVanillaPrefabs().ToList();
            else if (Levels.CurrentLevel.IsVanilla())
            {
                int index = 0;
                foreach (GameObject prefab in __instance.characterPacks)
                {
                    if (prefab == null) goto next;
                    prefab.GetOrAddComponent<CharacterIdentifiable>().Id = (Character)index;
                    prefab.Prefabitize();
                    CharacterRegistry.AddPrefab((Character)index, prefab);
                next:
                    index++;
                }
            }
            foreach (KeyValuePair<Character,GameObject> kvp in CharacterRegistry.objectsToPatch)
            {
                Character id = kvp.Key;
                GameObject prefab = kvp.Value;
                int idnt = (int)id;
                try { if (__instance.characterPacks[idnt]) __instance.characterPacks[idnt] = prefab; }
                catch
                {
                    for (int i = 0; i < (idnt + 1); i++)
                    {
                        if (__instance.characterPacks.Count < i)
                        {
                            __instance.characterPacks.Add(null);
                        }
                    }
                    __instance.characterPacks.Add(prefab);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Start")]
    internal static class PlayerStartPatch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(PlayerScript __instance)
        {
            try
            {
                __instance.EditedStart();
            }
            catch (Exception ex)
            {
                SALT.Console.Console.LogError(ex.ParseTrace());
            }
            return false;
        }

        private static IEnumerator RespawnAfterNative(PlayerScript player, ModdedLevelButtonScript mlbs)
        {
            yield return new WaitForEndOfFrame();
            Transform spawnPoint = mlbs.Spawnpoint;
            if (spawnPoint != null)
                player.checkpoint = spawnPoint;
            else
                Console.Console.LogWarning(mlbs.levelEnum + " does have a checkpoint script");
            player.Respawn();
            CheckpointScript.startCheckpoint.position = MainScript.spawnPoint;

        }

        private static void EditedSpawnCharacter(this PlayerScript __instance, int i)
        {
            PlayerSpawnCharacterPatch.Prefix(__instance, i);
            if (i < 0 || i >= __instance.characterPacks.Count)
            {
                PlayerSpawnCharacterPatch.Postfix(__instance, i);
                return;
            }
            if (__instance.currentCharacterPack != null)
                UnityEngine.Object.Destroy(__instance.currentCharacterPack);
            Console.Console.Log(__instance.anim.runtimeAnimatorController.name);
            GameObject gameObject = __instance.characterPacks[i].Instantiate<GameObject>( __instance.transform.position, __instance.transform.rotation, __instance.transform, true);
            __instance.anim.runtimeAnimatorController = gameObject.GetComponent<CharacterPack>().anim;
            Console.Console.Log(__instance.anim.runtimeAnimatorController.name);
            __instance.lastCharChange = Time.time;
            __instance.currentCharacterPack = gameObject;
            PlayerSpawnCharacterPatch.Postfix(__instance, i);
        }

        private static void EditedStart(this PlayerScript __instance)
        {
            Console.Console.Log("Currents: " + Levels.CurrentLevel);
            Main.player = __instance.gameObject;
            PlayerScript.player = __instance;
            __instance.anim = __instance.GetComponent<Animator>();
            __instance.rb = __instance.GetComponent<Rigidbody2D>();
            __instance.coll = __instance.GetComponent<BoxCollider2D>();
            __instance.aSource = __instance.GetComponent<AudioSource>();
            __instance.SpawnCharacter(MainScript.currentCharacter);
            __instance.Respawn();
            if (Levels.isMainMenu())
            {
                if (MainScript.spawnPoint == Vector3.zero)
                {
                    __instance.anim.SetTrigger("Title");
                    __instance.currentState = PlayerState.Title;
                }
                else
                {
                    if (LevelManager.levelManager.spawnPoints.ContainsKey(MainScript.lastLevelName))
                        __instance.checkpoint = LevelManager.levelManager.spawnPoints[MainScript.lastLevelName];
                    else
                    {
                        if (ModdedLevelButtonScript.buttons.LastIfContains(m => m.levelEnum.ToTitle() == MainScript.lastLevelName, out ModdedLevelButtonScript mlbs, null))
                        {
                            if (mlbs.IsNativeObjectAlive())
                            {
                                Transform spawnPoint = mlbs.Spawnpoint;
                                if (spawnPoint != null)
                                    __instance.checkpoint = spawnPoint;
                                else
                                    Console.Console.LogWarning(mlbs.levelEnum + " does have a checkpoint script");
                            }
                            else
                            {
                                __instance.StartCoroutine(RespawnAfterNative(__instance, mlbs));
                                return;
                            }
                        }
                        else
                            Console.Console.LogWarning(mlbs.levelEnum + " does not have a spawnpoint");
                    }
                    __instance.Respawn();
                    CheckpointScript.startCheckpoint.position = MainScript.spawnPoint;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("SpawnCharacter")]
    internal static class PlayerSpawnCharacterPatch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(PlayerScript __instance, int i)
        {
            Main.player = __instance.gameObject;
            if (i < 0 || i >= __instance.characterPacks.Count)
                return true;
            Character character = EnumUtils.FromInt<Character>(i);
            //Console.Console.LogSuccess("Spawning " + character.ToFriendlyName());
            if (__instance.characterPacks[i] == null)
            {
                //Console.Console.LogSuccess(character.ToFriendlyName() + " == null");
                if (__instance.currentCharacterPack != null)
                    UnityEngine.Object.Destroy(__instance.currentCharacterPack);
                //__instance.anim.runtimeAnimatorController = null;
                //__instance.lastCharChange = Time.time;
                //__instance.currentCharacterPack = null;
                return false;
            }
            return true;
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix(PlayerScript __instance, int i)
        {
            Main.player = __instance.gameObject;
            if (i < 0 || i >= __instance.characterPacks.Count)
                return;
            Character character = EnumUtils.FromInt<Character>(i);
            //Console.Console.LogSuccess("Spawned " + character.ToFriendlyName());
            if (__instance.characterPacks[i] == null)
            {
                return;
            }
            __instance.currentCharacterPack.SetActive(true);
            CharacterIdentifiable.AddIdentifiable(__instance.currentCharacterPack, character);
            Callbacks.OnCharacterSpawned_Trigger(__instance, character);
            return;
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("NextCharacter")]
    internal static class PlayerNextCharacterPatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Postfix(PlayerScript __instance)
        {
            Main.player = __instance.gameObject;
            //if (__instance.GetComponent<Rigidbody2D>().velocity.magnitude <= 0.1)
            //    __instance.SpawnCharacter(MainScript.currentCharacter);
            if (MainScript.currentCharacter == Character.NONE.ToInt())
                Main.NextCharacter();
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Title")]
    internal static class PlayerTitlePatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Postfix(PlayerScript __instance)
        {
            Main.player = __instance.gameObject;
            __instance.transform.localScale = __instance.flipVector * __instance.size;
        }
    }

    //[HarmonyPatch(typeof(CharacterIdentifiable))]
    //[HarmonyPatch("Awake")]
    //internal static class CharIdPatch
    //{
    //    [HarmonyPriority(Priority.First)]
    //    public static void Postfix(CharacterIdentifiable __instance)
    //    {
    //    }
    //}

}
