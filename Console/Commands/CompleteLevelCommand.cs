﻿using SALT.Extensions;
using UnityEngine;

namespace SALT.Console.Commands
{
    internal class CompleteLevelCommand : ConsoleCommand
    {
        public override string ID => "completelevel";

        public override string Usage => "completelevel";

        public override string Description => "Completes a level if a 'mom' button is available.";

        public override bool Execute(string[] args)
        {
            if (Levels.isMainMenu())
            {
                Console.LogError("Cannot complete a level when there is no level to complete.");
                return false;
            }
            if (Levels.isRedHeart())
            {
                var FakeBtn = Object.FindObjectOfType<FakeButtonScript>();
                if (FakeBtn != null)
                    FakeBtn.Pound();
                else
                    Console.LogError("No fake button found.");
                var potClose = Object.FindObjectOfType<PotCloseTrigger>();
                if (potClose != null)
                {
                    SAObjects.GetRootGameObject("PotTrap").SetActiveRecursivelyExt(true);
                    Patches.PotClosedPatch.OnPotClosed += CompleteRedHeart;
                    potClose.OnTriggerEnter2D(Main.CreatePlayerCollider());
                    return true;
                }
                else
                    Console.LogError("No pot found.");
            }
            return CompleteLevel();//Levels.isRedHeart() ? CompleteRedHeart() : CompleteLevel();
        }

        private static void CompleteRedHeart()
        {
            CompleteLevel();
            Patches.PotClosedPatch.OnPotClosed -= CompleteRedHeart;
        }

        private static bool CompleteLevel()
        {
            var ClearBtn = Object.FindObjectOfType<LevelClearButton>();
            if (ClearBtn == null)
            {
                Console.LogError("Failed to complete level. No 'mom' button found.");
                return false;
            }
            Main.StopSave();
            LevelManager.levelManager.deaths = 0;
            LevelManager.levelManager.bubbaTokens = new bool[3] { true, true, true };
            LevelManager.levelManager.collectedMoustaches = LevelManager.levelManager.totalMoustaches;
            MainScript.main.levelTime = 0.0f;
            ClearBtn.Pound();
            Console.LogSuccess("Successfully completed level.");
            return true;
        }
    }
}
