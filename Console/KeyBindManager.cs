using SALT.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SALT.Console
{
    internal static class KeyBindManager
    {
        private static List<KeyBindManager.KeyBinding> customKeyBinds = new List<KeyBindManager.KeyBinding>();
        private const string FILENAME = "consolekeybindings";

        public static void Init()
        {
            UserInputService.Instance.InputBegan += ((io, gp) =>
            {
                foreach (KeyBinding customKeyBind in KeyBindManager.customKeyBinds)
                {
                    if (gp) return;
                    if (customKeyBind.action == io.keyCode)
                        Console.ProcessInput(customKeyBind.commandToRun, true);
                }
            });
        }

        public static void ReadBinds()
        {
            string path = Path.Combine(FileSystem.GetMyConfigPath(), FILENAME);
            if (!File.Exists(path))
                return;
            KeyBindManager.customKeyBinds.Clear();
            using (BinaryReader reader = new BinaryReader((Stream)File.Open(path, FileMode.Open)))
                BinaryUtils.ReadList<KeyBindManager.KeyBinding>(reader, KeyBindManager.customKeyBinds, (Func<BinaryReader, KeyBindManager.KeyBinding>)(x => KeyBindManager.KeyBinding.Read(x)));
        }

        public static void SaveBinds()
        {
            using (BinaryWriter writer = new BinaryWriter((Stream)File.Open(Path.Combine(FileSystem.GetMyConfigPath(), FILENAME), FileMode.OpenOrCreate)))
                BinaryUtils.WriteList<KeyBindManager.KeyBinding>(writer, KeyBindManager.customKeyBinds, (Action<BinaryWriter, KeyBindManager.KeyBinding>)((x, y) => y.Write(x)));
        }

        public static void CreateBinding(string commandToRun, KeyCode bindingKey)
        {
            KeyBindManager.KeyBinding keyBinding = KeyBindManager.customKeyBinds.FirstOrDefault<KeyBindManager.KeyBinding>((Func<KeyBindManager.KeyBinding, bool>)(x => x.action == bindingKey));
            if (keyBinding != null)
            {
                keyBinding.commandToRun = commandToRun;
            }
            else
            {
                KeyBindManager.customKeyBinds.Add(new KeyBindManager.KeyBinding()
                {
                    action = bindingKey,
                    commandToRun = commandToRun
                });
            }
        }

        public static void RemoveBinding(KeyCode key)
        {
            KeyBindManager.KeyBinding keyBinding = KeyBindManager.customKeyBinds.FirstOrDefault<KeyBindManager.KeyBinding>((Func<KeyBindManager.KeyBinding, bool>)(x => x.action == key));
            if (keyBinding == null)
                return;
            KeyBindManager.customKeyBinds.Remove(keyBinding);
        }

        private class KeyBinding
        {
            public KeyCode action;
            public string commandToRun;

            public void Write(BinaryWriter writer)
            {
                writer.Write((int)this.action);
                writer.Write(this.commandToRun);
            }

            public static KeyBindManager.KeyBinding Read(BinaryReader reader)
            {
                return new KeyBindManager.KeyBinding()
                {
                    action = (KeyCode)reader.ReadInt32(),
                    commandToRun = reader.ReadString()
                };
            }
        }
    }
}
