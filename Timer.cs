using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using SALT.Extensions;
using UnityEngine;
using HarmonyLib;

namespace SALT
{
    public class Timer
    {
        private static bool isPaused = false;
        public static void Pause(bool _) => TogglePause(true, _);
        public static void Unpause(bool _) => TogglePause(false, _);
        private static void TogglePause(bool torf, bool affectCursor)
        {
            isPaused = torf;
            UserInputService.MouseVisible = affectCursor ? torf : false;
            if (!MainScript.paused)
                Time.timeScale = torf ? 0 : 1;
        }

        public static bool HasPauser() => isPaused;

        public static bool IsPaused() => isPaused || MainScript.paused;
    }
#if PAUSE_PATCH
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Update")]
    public static class PlayerPause_Patch
    {
        private static bool IsPauseField(CodeInstruction v)
        {
            if (v.opcode == OpCodes.Ldsfld)
            {
                System.Reflection.FieldInfo info = v.operand as System.Reflection.FieldInfo;
                if (info.Name == "paused")
                    return true;
            }
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(
          IEnumerable<CodeInstruction> instr) => instr.AddRangeAndRemoveWhere(new List<CodeInstruction>{ new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(Timer), "IsPaused")) }, IsPauseField);
        {
            foreach (CodeInstruction codeInstruction in instr)
            {
                CodeInstruction v = codeInstruction;
                if (v.opcode == OpCodes.Ldsfld)
                {
                    System.Reflection.FieldInfo info = v.operand as System.Reflection.FieldInfo;
                    if (info.Name == "paused")
                        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof(Timer), "IsPaused"));
                    else
                        yield return v;
                }
                else
                    yield return v;
            }
        }
    }
#endif
}