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
        public static void Setup_Patch(MeleeWeaponFirstPerson __instance)
        {
            if (__instance.gameObject.GetComponent<PlayerMeleeHitboxes>() == null)
                __instance.gameObject.AddComponent<PlayerMeleeHitboxes>().melee = __instance;
        }
    }
}
