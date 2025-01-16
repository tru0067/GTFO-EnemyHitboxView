using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using Enemies;
using Il2CppInterop.Runtime.Injection;
using CConsole.Interop;
using Il2CppInterop.Runtime.InteropTypes;
using FluffyUnderware.DevTools.Extensions;
using Gear;
using EnemyHitboxView.Components;

namespace EnemyHitboxView
{
    [BepInPlugin("JarheadHME.EnemyHitboxView", "EnemyHitboxView", VersionInfo.Version)]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("CConsole", BepInDependency.DependencyFlags.HardDependency)]
    internal class EntryPoint : BasePlugin
    {
        private Harmony _Harmony = null;

        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<EnemyLimbHitboxes>();
            ClassInjector.RegisterTypeInIl2Cpp<EnemyHitboxCuller>();
            ClassInjector.RegisterTypeInIl2Cpp<EnemyMeleeHitboxes>();
            ClassInjector.RegisterTypeInIl2Cpp<EnemyBackMulti>();
            ClassInjector.RegisterTypeInIl2Cpp<PlayerMeleeHitboxes>();

            _Harmony = new Harmony($"{VersionInfo.RootNamespace}.Harmony");
            _Harmony.PatchAll();
            Logger.Info($"Plugin has loaded with {_Harmony.GetPatchedMethods().Count()} patches!");

            CustomCommands.Register(new()
            {
                Command = "DisplayEnemyBackMulti",
                Description = "Show the vectors involved in the back multi calculation. Enemy's forward is green, enemy's chest is blue.",
                Usage = "DisplayEnemyBackMulti [true/false]",
                Category = CConsole.Commands.CategoryType.Enemy,
                MinArgumentCount = 0
            },
            (in CustomCmdContext context, string[] args) =>
            {
                if (args.Length > 0 && bool.TryParse(args[0], out bool result))
                    EnemyBackMulti.ShowVectors = result;
                else
                    EnemyBackMulti.ShowVectors ^= true; // toggle

                context.Log($"DisplayEnemyBackMulti: {EnemyBackMulti.ShowVectors}");
            }
            );
            CustomCommands.Register(new()
            {
                Command = "DisplayEnemyHitboxes",
                Description = "Show spheres and capsules on enemies to mark their (approximate) hitboxes. Normal hitboxes are green, weakpoints are red, and armor is grey.",
                Usage = "DisplayEnemyHitboxes [true/false]",
                Category = CConsole.Commands.CategoryType.Enemy,
                MinArgumentCount = 0
            },
            (in CustomCmdContext context, string[] args) =>
                {
                    if (args.Length > 0 && bool.TryParse(args[0], out bool result))
                        EnemyLimbHitboxes.ShowHitboxes = result;
                    else
                        EnemyLimbHitboxes.ShowHitboxes ^= true; // toggle

                    context.Log($"DisplayEnemyHitboxes: {EnemyLimbHitboxes.ShowHitboxes}");
                }
            );
            CustomCommands.Register(new()
            {
                Command = "DisplayEnemyMeleeHitboxes",
                Description = "Show spheres to mark enemy's melee hitboxes.",
                Usage = "DisplayEnemyMeleeHitboxes [true/false]",
                Category = CConsole.Commands.CategoryType.Enemy,
                MinArgumentCount = 0
            },
            (in CustomCmdContext context, string[] args) =>
            {
                if (args.Length > 0 && bool.TryParse(args[0], out bool result))
                    EnemyMeleeHitboxes.ShowHitboxes = result;
                else
                    EnemyMeleeHitboxes.ShowHitboxes ^= true; // toggle

                context.Log($"DisplayEnemyMeleeHitboxes: {EnemyMeleeHitboxes.ShowHitboxes}");
            }
            );
            CustomCommands.Register(new()
            {
                Command = "DisplayPlayerMeleeHitboxes",
                Description = "Show spheres to mark player's melee hitboxes.",
                Usage = "DisplayPlayerMeleeHitboxes [true/false]",
                Category = CConsole.Commands.CategoryType.LocalPlayer,
                MinArgumentCount = 0
            },
            (in CustomCmdContext context, string[] args) =>
            {
                if (args.Length > 0 && bool.TryParse(args[0], out bool result))
                    PlayerMeleeHitboxes.ShowHitboxes = result;
                else
                    PlayerMeleeHitboxes.ShowHitboxes ^= true; // toggle

                context.Log($"DisplayPlayerMeleeHitboxes: {PlayerMeleeHitboxes.ShowHitboxes}");
            }
            );
        }

        public override bool Unload()
        {
            _Harmony.UnpatchSelf();
            return base.Unload();
        }
    }
}