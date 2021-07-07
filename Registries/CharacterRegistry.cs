﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SALT;
using SALT.Extensions;
using static SALT.ModLoader;

namespace SALT.Registries
{
    public static class CharacterRegistry
    {
        internal static Dictionary<Character, GameObject> objectsToPatch = new Dictionary<Character, GameObject>(); 
        internal static Dictionary<Character, Sprite> spritesToPatch = new Dictionary<Character, Sprite>();

        private static int LAST_FREE_ID = -1;
        private static Dictionary<int,string> moddedIds = new Dictionary<int, string>();
        public static string[] GetNames()
        {
            List<string> names = Enum.GetNames(typeof(Character)).ToList();
            var keys = moddedIds.Keys.ToList();
            keys.BasicSort();
            foreach (int idnt in keys)
                names.Add(moddedIds[idnt]);
            return names.ToArray();
        }
        public static string GetNameFromInt(int idnt)
        {
            if (idnt <= (int)Character.NONE)
                return ((Character)idnt).ToString();
            if (moddedIds.ContainsKey(idnt))
                return moddedIds[idnt];
            else
                return "NONE";
        }

        public static Character GetCharacterFromName(string name)
        {
            try
            {
                return (Character)Enum.Parse(typeof(Character), name);
            }
            catch
            {
                foreach (KeyValuePair<int, string> kvp in moddedIds)
                {
                    int idnt = kvp.Key;
                    string idname = kvp.Value;
                    if (name == idname)
                        return GetFromInt(idnt);
                }
                return Character.NONE;
            }
        }

        public static int GetIntFromName(string name)
        {
            return (int)GetCharacterFromName(name);
        }

        private static Dictionary<Character, GameObject> allPrefabs = new Dictionary<Character, GameObject>();
        private static Dictionary<Character, Sprite> allSprites = new Dictionary<Character, Sprite>();

        internal static Dictionary<Character, GameObject> AllPrefabs
        {
            get
            {
                var keys = allPrefabs.Keys.ToList().EnumToInt().IntToEnum<Character>();
                Dictionary<Character, GameObject> newList = new Dictionary<Character, GameObject>();
                foreach (Character c in keys)
                    newList.Add(c, allPrefabs[c]);
                return newList;
            }
        }

        /// <summary>
        /// Gets the first free space in the enum
        /// </summary>
        /// <returns>The value of that free space</returns>
        private static Character FindFree()
        {
            Character result;

            while (true)
            {
                LAST_FREE_ID++;

                object obj = ToObject(LAST_FREE_ID);
                result = (Character)obj; 

                if (!IsDefined(obj)) break;
            }

            return result;
        }

        private static Character RegisterValueWithEnum(string name)
        {
            Character free = FindFree();
            moddedIds.Add((int)free, name);
            return free;
        }

        private static object ToObject(int num) => Enum.ToObject(typeof(Character), num);

        internal static Character GetFromInt(int num) => (Character)ToObject(num);

        internal static bool IsDefined(object obj) => Enum.IsDefined(typeof(Character), obj) || moddedIds.ContainsKey((int)(Character)obj);

        internal static bool IsDefined(string name)
        {
            if (Enum.IsDefined(typeof(Character), name))
                return true;
            foreach (KeyValuePair<int, string> kvp in moddedIds)
            {
                string idname = kvp.Value;
                if (name == idname)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a new enum id with a name you specify.
        /// </summary>
        /// <param name="name">The name of the character. Will be capitalized and spaces will be replaced with _</param>
        /// <returns>A new character enum</returns>
        /// <exception cref="LoadingStepException">
        /// Happens if you call this method after <see cref="LoadingStep.PRELOAD"/>
        /// </exception>
        public static Character CreateCharacterId(string name)
        {
            if (ModLoader.CurrentLoadingStep > LoadingStep.PRELOAD)
                throw new LoadingStepException("Can't register identifiables outside of the PreLoad step");
            return RegisterValueWithEnum(name.ToUpper().Replace(" ","_"));
        }

        /// <summary>
        /// Register a (<see cref="CharacterPack"/> / <see cref="CharacterIdentifiable"/>) prefab into <see cref="PlayerScript.characterPacks"/>
        /// </summary>
        /// <param name="b"></param>
        public static void RegisterCharacterPrefab(GameObject b)
        {
            Character id = CharacterIdentifiable.GetId(b);
            AddPrefab(id, b);
            switch (CurrentLoadingStep)
            {
                case LoadingStep.PRELOAD:
                    if (!objectsToPatch.ContainsKey(id))
                        objectsToPatch.Add(id, b);
                    else
                        objectsToPatch[id] = b;
                    break;
                default:
                    if (!objectsToPatch.ContainsKey(id))
                        objectsToPatch.Add(id, b);
                    else
                        objectsToPatch[id] = b;
                    int idnt = (int)id;
                    try { if (Main.actualPlayer.characterPacks[idnt]) Main.actualPlayer.characterPacks[idnt] = b; }
                    catch
                    {
                        for (int i = 0; i < (idnt + 1); i++)
                        {
                            if (Main.actualPlayer.characterPacks.Count < i)
                            {
                                Main.actualPlayer.characterPacks.Add(null);
                            }
                        }
                        Main.actualPlayer.characterPacks.Add(b);
                    }
                    break;
            }
        }

        public static void RegisterCharacterPrefab(Character id, GameObject b)
        {
            if (b.HasComponent<CharacterIdentifiable>())
                b.GetComponent<CharacterIdentifiable>().Id = id;
            else
                b.AddComponent<CharacterIdentifiable>().Id = id;
            AddPrefab(id, b);
            switch (CurrentLoadingStep)
            {
                case LoadingStep.PRELOAD:
                    if (!objectsToPatch.ContainsKey(id))
                        objectsToPatch.Add(id, b);
                    else
                        objectsToPatch[id] = b;
                    break;
                default:
                    if (!objectsToPatch.ContainsKey(id))
                        objectsToPatch.Add(id, b);
                    else
                        objectsToPatch[id] = b;
                    int idnt = (int)id;
                    try { if (Main.actualPlayer.characterPacks[idnt]) Main.actualPlayer.characterPacks[idnt] = b; }
                    catch
                    {
                        for (int i = 0; i < (idnt + 1); i++)
                        {
                            if (Main.actualPlayer.characterPacks.Count < i)
                            {
                                Main.actualPlayer.characterPacks.Add(null);
                            }
                        }
                        Main.actualPlayer.characterPacks.Add(b);
                    }
                    break;
            }
        }

        public static void RegisterCharacterPrefab(CharacterIdentifiable b)
        {
            RegisterCharacterPrefab(b.gameObject);
        }

        internal static List<GameObject> GetPrefabs() => AllPrefabs.Values.ToList();

        internal static void AddPrefab(Character id, GameObject go)
        {
            if (!allPrefabs.ContainsKey(id))
                allPrefabs.Add(id, go);
            else
                allPrefabs[id] = go;
        }

        internal static void AddSprite(Character id, Sprite spr)
        {
            if (!allSprites.ContainsKey(id))
                allSprites.Add(id, spr);
            else
                allSprites[id] = spr;
        }

        public static void RegisterCharacterSprite(Character id, Sprite spr)
        {
            switch (CurrentLoadingStep)
            {
                case LoadingStep.PRELOAD:
                    if (!spritesToPatch.ContainsKey(id))
                        spritesToPatch.Add(id, spr);
                    else
                        spritesToPatch[id] = spr;
                    AddSprite(id, spr);
                    break;
                default:
                    if (!spritesToPatch.ContainsKey(id))
                        spritesToPatch.Add(id, spr);
                    else
                        spritesToPatch[id] = spr;
                    int idnt = (int)id;
                    Main.characterOption.charIcons[idnt] = spr;
                    AddSprite(id, spr);
                    break;
            }
        }

        /// <summary>
        /// Gets a character pack with the id you specify.
        /// </summary>
        /// <param name="id">Character id to get the prefab for</param>
        /// <returns>The prefab that matchs the <paramref name="id"/></returns>
        /// <exception cref="LoadingStepException">
        /// Happens if you call this method during <see cref="LoadingStep.PRELOAD"/>
        /// </exception>
        public static GameObject GetCharacter(Character id)
        {
            if (ModLoader.CurrentLoadingStep == LoadingStep.PRELOAD)
                throw new LoadingStepException("Can't get character prefab during preload");
            return Main.actualPlayer.characterPacks.FirstOrDefault(c => CharacterIdentifiable.GetId(c) == id);
        }

        /// <summary>
        /// Gets a character icon with the id you specify.
        /// </summary>
        /// <param name="id">Character id to get the sprite for</param>
        /// <returns>The sprite that matchs the <paramref name="id"/></returns>
        /// <exception cref="LoadingStepException">
        /// Happens if you call this method during <see cref="LoadingStep.PRELOAD"/>
        /// </exception>
        public static Sprite GetIcon(Character id)
        {
            if (ModLoader.CurrentLoadingStep == LoadingStep.PRELOAD)
                throw new LoadingStepException("Can't get character icon during preload");
            return allSprites.ContainsKey(id) ? allSprites[id] : null;
        }

        internal static void AddAndRemoveWhere<T>(this List<T> list, T value, Func<T, T, bool> cond)
        {
            var v = list.Where(x => cond(value, x)).ToList();
            foreach (var a in v)
            {
                list.Remove(a);
            }
            list.Add(value);
        }

        public static Character Parse(string value) => GetCharacterFromName(value);
    }
}