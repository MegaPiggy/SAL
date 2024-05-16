using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using SALT.Extensions;
using SALT.Registries;

namespace SALT.Patches
{
    [HarmonyPatch(typeof(CharacterOption))]
    [HarmonyPatch("Awake")]
    internal static class CharacterOptionAwakePatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(CharacterOption __instance)
        {
            Main.characterOption = __instance;
            __instance.po = __instance.GetComponent<PauseOption>();
            int index = 0;
            foreach (Sprite spr in __instance.charIcons)
            {
                CharacterRegistry.AddSprite((Character)index, spr);
                index++;
            }
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix(CharacterOption __instance)
        {
            Main.characterOption = __instance;
            var selectionStrings = new List<string>();
            foreach (Character character in EnumUtils.GetAll<Character>())
            {
                if (__instance.charIcons.ElementAtOrDefault(character.ToInt()) == null)
                {
                    if (CharacterRegistry.spritesToPatch.TryGetValue(character, out Sprite spr))
                        __instance.charIcons.Add(spr);
                    else
                    {
                        __instance.charIcons.Add(null);
                        if (character != Character.NONE)
                            (character.ToFriendlyName() + " does not have sprite").Log();
                    }
                }
                selectionStrings.Add(" ");
            }
            __instance.po.selectionStrings = selectionStrings;
        }
    }

    [HarmonyPatch(typeof(CharacterOption))]
    [HarmonyPatch("Start")]
    internal static class CharacterOptionStartPatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(CharacterOption __instance)
        {
            Main.characterOption = __instance;

            if (MainScript.currentCharacter == Character.NONE.ToInt())
            {
                int num = Character.AMELIA.ToInt();
                MainScript.currentCharacter = num;
                PlayerPrefs.SetInt("character", num);
                Main.actualPlayer.currentCharacter = num;
                __instance.po.currentSelection = num;
            }
            if ((__instance.charIcons.Count - 1) < MainScript.currentCharacter)
            {
                int num = Character.AMELIA.ToInt();
                MainScript.currentCharacter = num;
                PlayerPrefs.SetInt("character", num);
                Main.actualPlayer.currentCharacter = num;
                __instance.po.currentSelection = num;
            }
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix(CharacterOption __instance)
        {
            Main.characterOption = __instance;
            __instance.po.currentSelection = MainScript.currentCharacter;
            __instance.charSprite.sprite = __instance.charIcons[MainScript.currentCharacter];
        }
    }

    [HarmonyPatch(typeof(CharacterOption))]
    [HarmonyPatch("Update")]
    internal static class CharacterOptionUpdatePatch
    {
        public static int lastSelected = Character.NONE.ToInt();

        [HarmonyPriority(Priority.First)]
        public static void Prefix(CharacterOption __instance)
        {
            Main.characterOption = __instance;
            //if (__instance.po.currentSelection != lastSelected)
            //{
            //    (lastSelected.ToCharacter().ToFriendlyName() + "=>" + __instance.po.currentSelection.ToCharacter().ToFriendlyName()).Log();
            //}
            if (__instance.po.currentSelection == Character.NONE.ToInt())
            {
                if (lastSelected == Character.NONE.ToInt()-1)
                    __instance.po.currentSelection = Character.NONE.ToInt()+1;
                else if (lastSelected == (Character.NONE.ToInt() + 1) || lastSelected == Character.AMELIA.ToInt())
                    __instance.po.currentSelection = Character.NONE.ToInt()-1;
                else
                    __instance.po.currentSelection = Character.AMELIA.ToInt();
            }

            if ((__instance.charIcons.Count - 1) < __instance.po.currentSelection)
            {
                __instance.po.currentSelection = Character.AMELIA.ToInt();
            }

            lastSelected = __instance.po.currentSelection;
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix(CharacterOption __instance)
        {
            Main.characterOption = __instance;
            __instance.po.currentSelection = MainScript.currentCharacter;
            __instance.charSprite.sprite = __instance.charIcons[MainScript.currentCharacter];
        }
    }

    [HarmonyPatch(typeof(CharacterOption))]
    [HarmonyPatch("OnDestroy")]
    internal static class CharacterOptionDestroyPatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix()
        {
            Main.characterOption = null;
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix()
        {
            Main.characterOption = null;
        }
    }

    [HarmonyPatch(typeof(VsyncOption))]
    [HarmonyPatch("Awake")]
    internal static class VsyncAwakePatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(VsyncOption __instance)
        {
            GameObject option = Utils.PrefabUtils.CopyPrefab(__instance.gameObject);
            option.name = "Option";
            option.RemoveComponentImmediate<VsyncOption>();
            PauseOption po = option.GetComponent<PauseOption>();
            po.label = "Placeholder";
            po.EditLabels("Placeholder");
            po.SetSelections(new List<string> { "" });
            po.currentSelection = 0;
            po.Update();
            po.RemoveAllMethods();
            po.SetLabelText(false);
            SAObjects.OptionPrefab = option;
            RectTransform vsyncRT = __instance.gameObject.GetComponent<RectTransform>();
            GameObject vfxOption = UnityEngine.Object.Instantiate(SAObjects.OptionPrefab, vsyncRT.position, vsyncRT.rotation, vsyncRT.parent);
            RectTransform vfxRT = vfxOption.GetComponent<RectTransform>();
            vfxOption.name = "VFXOption";
            vfxRT.localPosition = vsyncRT.localPosition.SetY(-787.5f);
            var vfx = vfxOption.AddComponent<VFXOption>();
            PauseOption vpo = vfxOption.GetComponent<PauseOption>();
            vpo.label = "VFX";
            vpo.EditLabels("VFX");//, "視覚効果");
            vpo.AddMethod(vfx.SetVFX);
            vpo.SetLabelText(false);
            vfxOption.SetActive(true);
            //Utils.DumpUtils.DumpObject(__instance.transform.parent.gameObject);
            __instance.po = __instance.GetComponent<PauseOption>();
            __instance.po.EditLabels("Framerate Cap", "フレームレート制限");
        }
    }

    [HarmonyPatch(typeof(VsyncOption))]
    [HarmonyPatch("SetStrings")]
    internal static class VsyncSetStringsPatch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(VsyncOption __instance)
        {
            List<string> stringList = new List<string>();
            if (MainScript.language == Language.Japanese)
            {
                stringList.Add("垂直同期オン");
                stringList.Add("リフレッシュレート");
                stringList.Add("なし");
                stringList.Add("60fps");
                stringList.Add("100fps");
                stringList.Add("144fps");
            }
            else
            {
                stringList.Add("Vsync On");
                stringList.Add("Refresh Rate");
                stringList.Add("Uncapped");
                stringList.Add("60fps");
                stringList.Add("100fps");
                stringList.Add("144fps");
            }
            __instance.po.selectionStrings = stringList;
            __instance.currentLanguage = MainScript.language;
            return false;
        }
    }

    [HarmonyPatch(typeof(VsyncOption))]
    [HarmonyPatch("SetVsync")]
    internal static class SetVsyncPatch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(VsyncOption __instance)
        {
            if (__instance.po.currentSelection == 0)
                MainScript.SetFramerateLock(-1);
            else if (__instance.po.currentSelection == 1)
                MainScript.SetFramerateLock(Screen.currentResolution.refreshRate);
            else if (__instance.po.currentSelection == 2)
                MainScript.SetFramerateLock(0);
            else if (__instance.po.currentSelection == 3)
                MainScript.SetFramerateLock(60);
            else if (__instance.po.currentSelection == 4)
                MainScript.SetFramerateLock(100);
            else if (__instance.po.currentSelection == 5)
                MainScript.SetFramerateLock(144);
            PlayerPrefs.SetInt("vsync", __instance.po.currentSelection);
            return false;
        }
    }

    [HarmonyPatch(typeof(LanguageOption))]
    [HarmonyPatch("Awake")]
    internal static class LanguageOptionAwakePatch
    {

        [HarmonyPriority(Priority.First)]
        public static void Prefix(LanguageOption __instance)
        {
            if (PlayerPrefs.GetInt("Language", -1) == -1)
            {
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.Japanese:
                        PlayerPrefs.SetInt("Language", (int)Language.Japanese);
                        break;
                    default:
                        PlayerPrefs.SetInt("Language", (int)Language.English);
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(LanguageOption))]
    [HarmonyPatch("Update")]
    internal static class LanguageOptionUpdatePatch
    {
        [HarmonyPriority(Priority.First)]
        public static void Prefix(LanguageOption __instance)
        {
            //MainScript.language = __instance.languages[__instance.po.currentSelection];
        }
    }

    [HarmonyPatch(typeof(PauseOption))]
    [HarmonyPatch("Update")]
    internal static class PauseOptionUpdatePatch
    {
        internal static bool Prefix(PauseOption __instance)
        {
            if (string.IsNullOrEmpty(__instance.selectionStrings.ElementAtOrDefault(__instance.currentSelection)))
                return false;
            return true;
        }
    }
}
