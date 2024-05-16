using SALT.Extensions;
using SALT.Utils;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SALT
{
    internal static partial class Shortcuts
    {
        public static AssetBundle LoadAssetbundle(Mod mod, string folder, string name)
        {
            folder = folder.EndsWith(".") ? folder : $"{folder}.";
            string path = folder + name;
            Assembly assembly = mod.Assembly;
            Console.Console.LogWarning("Attempting to load asset bundle at path: " + path);
            Stream stream = assembly.GetManifestResourceStream(mod.EntryType, path);
            if (stream == null)
                throw new Exception("AssetBundle " + name + " was not found");
            AssetBundle assetBundle = AssetBundle.LoadFromStream(stream);
            if (assetBundle == null)
                Console.Console.LogError("AssetBundle " + name + " was loaded incorrectly!");
            return assetBundle;
        }

        public static AssetBundle LoadAssetbundle(Mod mod, string name)
        {
            string path = name;
            Assembly assembly = mod.Assembly;
            Console.Console.LogWarning("Attempting to load asset bundle at path: " + path);
            Stream stream = assembly.GetManifestResourceStream(mod.EntryType, path);
            if (stream == null)
                throw new Exception("AssetBundle " + name + " was not found");
            AssetBundle assetBundle = AssetBundle.LoadFromStream(stream);
            if (assetBundle == null)
                Console.Console.LogError("AssetBundle " + name + " was loaded incorrectly!");
            return assetBundle;
        }

        public static Texture2D CreateTexture2DFromImage(Mod mod, string folder, string name)
        {
            folder = folder.EndsWith(".") ? folder : $"{folder}.";
            Assembly assembly = mod.Assembly;
            string manifestResourceName = folder + name;
            string realName = name.RemoveExtension();
            Stream manifestResourceStream = assembly.GetManifestResourceStream(mod.EntryType, manifestResourceName);
            Texture2D texture2D = new Texture2D(4, 4);
            byte[] numArray = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(numArray, 0, (int)manifestResourceStream.Length);
            texture2D.LoadImage(numArray);
            Console.Console.Log(realName + " vs " + manifestResourceName);
            texture2D.name = realName;
            manifestResourceStream.Close();
            return texture2D;
        }

        public static Texture2D CreateTexture2DFromImage(Mod mod, string name)
        {
            Assembly assembly = mod.Assembly;
            string manifestResourceName = name;
            string realName = name.RemoveExtension();
            Stream manifestResourceStream = assembly.GetManifestResourceStream(mod.EntryType, manifestResourceName);
            Texture2D texture2D = new Texture2D(4, 4);
            byte[] numArray = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(numArray, 0, (int)manifestResourceStream.Length);
            texture2D.LoadImage(numArray);
            texture2D.name = realName;
            manifestResourceStream.Close();
            return texture2D;
        }

        public static Sprite CreateSpriteFromImage(Mod mod, string folder, string name)
        {
            folder = folder.EndsWith(".") ? folder : $"{folder}.";
            string realName = name.RemoveExtension();
            Texture2D texture2D = CreateTexture2DFromImage(mod, folder, name);
            Sprite sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
            sprite.name = realName;
            return sprite;
        }

        public static Sprite CreateSpriteFromImage(Mod mod, string name)
        {
            string realName = name.RemoveExtension();
            Texture2D texture2D = CreateTexture2DFromImage(mod, name);
            Sprite sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
            sprite.name = realName;
            return sprite;
        }

        public static SpriteTexturePack CreateTexture2DAndSpriteFromImage(Mod mod, string folder, string name)
        {
            folder = folder.EndsWith(".") ? folder : $"{folder}.";
            string realName = name.RemoveExtension();
            Texture2D texture2D = CreateTexture2DFromImage(mod, folder, name);
            Sprite sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
            sprite.name = realName;
            SpriteTexturePack spriteTexturePack = ScriptableObject.CreateInstance<SpriteTexturePack>();
            spriteTexturePack.name = realName;
            spriteTexturePack.SetTexture(texture2D);
            spriteTexturePack.SetSprite(sprite);
            return spriteTexturePack;
        }

        public static SpriteTexturePack CreateTexture2DAndSpriteFromImage(Mod mod, string name)
        {
            string realName = name.RemoveExtension();
            Texture2D texture2D = CreateTexture2DFromImage(mod, name);
            Sprite sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
            sprite.name = realName;
            SpriteTexturePack spriteTexturePack = ScriptableObject.CreateInstance<SpriteTexturePack>();
            spriteTexturePack.name = realName;
            spriteTexturePack.SetTexture(texture2D);
            spriteTexturePack.SetSprite(sprite);
            return spriteTexturePack;
        }
    }
}