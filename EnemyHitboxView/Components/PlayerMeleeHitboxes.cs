using FluffyUnderware.DevTools.Extensions;
using Gear;
using UnityEngine;

namespace EnemyHitboxView.Components
{
    // Made by tru0067
    internal class PlayerMeleeHitboxes : MonoBehaviour
    {
        public static bool ShowHitboxes = false;

        public MeleeWeaponFirstPerson melee;
        public Renderer meleeHitbox;

        public void Start()
        {
            GameObject meleeHitboxGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meleeHitboxGO.GetComponent<Collider>().enabled = false;
            meleeHitbox = meleeHitboxGO.GetComponent<Renderer>();
            meleeHitbox.material.color = Color.white;
            meleeHitbox.transform.position = Vector3.zero;
        }

        public void OnDestroy()
        {
            if (meleeHitbox != null)
                meleeHitbox.gameObject.Destroy();
        }

        public void Update()
        {
            if (melee == null || meleeHitbox == null)
                return;

            MWS_Base currentState = melee.CurrentState;
            if (!ShowHitboxes || currentState == null)
            {
                meleeHitbox.enabled = false;
                return;
            }
            // Currently meleeing.
            MWS_AttackSwingBase currentSwing = currentState.TryCast<MWS_AttackSwingBase>();
            if (currentSwing != null
                && currentSwing.m_data.m_damageStartTime < currentSwing.m_elapsed
                && currentSwing.m_elapsed <= currentSwing.m_data.m_damageEndTime
                && !currentSwing.m_targetsFound)
            {
                float radius = melee.MeleeArchetypeData.AttackSphereRadius;
                // Check if the radius is getting enlarged for reasons.
                float inFront = Vector3.Dot(melee.Owner.FPSCamera.Forward, (melee.ModelData.m_damageRefAttack.position - melee.Owner.FPSCamera.Position).normalized);
                if (inFront > 0.5)
                    radius *= 1f + inFront * melee.m_attackDamageSphereDotScale;
                meleeHitbox.transform.localScale = new Vector3(radius, radius, radius);
                meleeHitbox.transform.position = currentSwing.m_data.m_damageRef.position;
                meleeHitbox.enabled = true;
                return;
            }
            // Currently shoving.
            MWS_Push currentPush = currentState.TryCast<MWS_Push>();
            if (currentPush != null
                && currentPush.m_data.m_damageStartTime < currentPush.m_elapsed
                && currentPush.m_elapsed <= currentPush.m_data.m_damageEndTime
                && !currentPush.m_damageDone)
            {
                float radius = melee.MeleeArchetypeData.PushDamageSphereRadius;
                meleeHitbox.transform.localScale = new Vector3(radius, radius, radius);
                meleeHitbox.transform.position = currentPush.m_data.m_damageRef.position;
                meleeHitbox.enabled = true;
                return;
            }
            // In theory `MWS_Hit` can also hit enemies for weapons with `CanHitMultipleEnemies`,
            // but the code for it has changed since R6Mono so I don't know how to implement a
            // valid check.

            // We're not doing anything with an active hitbox.
            meleeHitbox.enabled = false;
            return;
        }
    }
}
