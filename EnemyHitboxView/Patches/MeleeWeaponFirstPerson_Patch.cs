using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnemyHitboxView.Components;
using Gear;
using HarmonyLib;


namespace EnemyHitboxView.Patches
{
    [HarmonyPatch]
    internal class MeleeWeaponFirstPerson_Patch
    {
        [HarmonyPatch(typeof(MeleeWeaponFirstPerson), nameof(MeleeWeaponFirstPerson.Setup))]
        [HarmonyPostfix]
        public static void MWFPSetup_Patch(MeleeWeaponFirstPerson __instance)
        {
            if (__instance.gameObject.GetComponent<PlayerMeleeHitboxes>() == null)
                __instance.gameObject.AddComponent<PlayerMeleeHitboxes>().melee = __instance;
        }

    }
}
