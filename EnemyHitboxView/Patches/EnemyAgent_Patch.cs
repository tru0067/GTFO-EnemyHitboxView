using Enemies;
using EnemyHitboxView.Components;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnemyHitboxView.Patches
{
    [HarmonyPatch]
    internal class EnemyAgent_Patch
    {

        [HarmonyPatch(typeof(EnemyAgent), nameof(EnemyAgent.Setup))]
        [HarmonyPostfix]
        public static void Setup_Postfix(EnemyAgent __instance)
        {
            // 

            CheckAndAddLimbHitboxComponents(__instance);
            // Add a component to the EnemyAgent to turn its hitbox renderers on and off.
            if (__instance.gameObject.GetComponent<EnemyHitboxCuller>() == null)
                __instance.gameObject.AddComponent<EnemyHitboxCuller>();

            // And one for its back multi vectors:
            if (__instance.gameObject.GetComponent<EnemyBackMulti>() == null)
                __instance.gameObject.AddComponent<EnemyBackMulti>().enemy = __instance;

            // And one for its melee hitboxes:
            if (__instance.gameObject.GetComponent<EnemyMeleeHitboxes>() == null)
                __instance.gameObject.AddComponent<EnemyMeleeHitboxes>().enemy = __instance;
        }

        public static void CheckAndAddLimbHitboxComponents(EnemyAgent __instance)
        {
            foreach (var limb in __instance.Damage.DamageLimbs)
            {
                GameObject go = limb.gameObject;

                // This patch seems to run twice, so make sure the component is just there once so it doesn't get doubled
                // If it's trying to double up, then just cut the entire method short to skip checking that on the rest
                if (go.GetComponent<EnemyLimbHitboxes>() != null)
                    return;

                Collider collider = go.GetComponent<Collider>();
                if (collider == null)
                {
                    Logger.Warn($"Damage Limb {go.name} on enemy {__instance.EnemyDataID} ({__instance.EnemyData.name}) doesn't have a collider?");
                    continue;
                }

                EnemyLimbHitboxes hitbox = go.AddComponent<EnemyLimbHitboxes>();
                hitbox.Setup(limb, collider);
            }
        }
    }
}
