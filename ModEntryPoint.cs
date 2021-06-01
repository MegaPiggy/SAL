﻿using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SALT
{
    public interface IModEntryPoint
    {
        void PreLoad();

        void Load();

        void PostLoad();
    }

    public abstract class ModEntryPoint : IModEntryPoint
    {
        public Harmony HarmonyInstance => HarmonyPatcher.GetInstance();

        public virtual void Load()
        {
        }

        public virtual void PostLoad()
        {
        }

        public virtual void PreLoad()
        {
        }
    }
}
