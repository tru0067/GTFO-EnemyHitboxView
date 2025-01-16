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

            // Find out if we are currently meleeing.
            bool currentlyMeleeing = true;
            if (!ShowHitboxes || !currentlyMeleeing)
            {
                meleeHitbox.enabled = false;
                return;
            }

            meleeHitbox.enabled = true;
            float radius = melee.MeleeArchetypeData.AttackSphereRadius;
            // Check if the radius is getting enlarged for reasons.
            float inFront = Vector3.Dot(melee.Owner.FPSCamera.Forward, (melee.ModelData.m_damageRefAttack.position - melee.Owner.FPSCamera.Position).normalized);
            if (inFront > 0.5)
                radius *= 1f + inFront * melee.m_attackDamageSphereDotScale;
            meleeHitbox.transform.localScale = new Vector3(radius, radius, radius);
            meleeHitbox.transform.position = melee.ModelData.m_damageRefAttack.position;
        }
    }

}
