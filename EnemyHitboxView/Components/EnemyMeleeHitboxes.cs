using Enemies;
using FluffyUnderware.DevTools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnemyHitboxView.Components
{
    internal class EnemyMeleeHitboxes : MonoBehaviour
    {
        public static bool ShowHitboxes = false;

        public EnemyAgent enemy;
        public Renderer left;
        public Renderer right;

        public void Start()
        {
            GameObject leftGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftGO.GetComponent<Collider>().enabled = false;
            left = leftGO.GetComponent<Renderer>();
            left.material.color = Color.red;
            left.transform.position = Vector3.zero;
            GameObject rightGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightGO.GetComponent<Collider>().enabled = false;
            right = rightGO.GetComponent<Renderer>();
            right.material.color = Color.red;
            right.transform.position = Vector3.zero;
        }

        public void OnDestroy()
        {
            if (left != null)
                left.gameObject.Destroy();
            if (right != null)
                right.gameObject.Destroy();
        }

        public void Update()
        {
            if (enemy == null || left == null || right == null)
                return;

            // Find out if the enemy is currently meleeing.
            ES_StrikerMelee strikerMelee = enemy.Locomotion.StrikerMelee;
            float meleeProgress = (Clock.Time - strikerMelee.m_startTime) * strikerMelee.m_animSpeed;
            bool currentlyMeleeing = strikerMelee.m_attackData.DamageStart < meleeProgress && meleeProgress < strikerMelee.m_attackData.DamageEnd;

            if (!ShowHitboxes || !enemy.Alive || !currentlyMeleeing)
            {
                left.enabled = false;
                right.enabled = false;
                return;
            }

            left.enabled = true;
            right.enabled = true;
            left.transform.localScale = new Vector3(strikerMelee.m_damageRad, strikerMelee.m_damageRad, strikerMelee.m_damageRad);
            right.transform.localScale = new Vector3(strikerMelee.m_damageRad, strikerMelee.m_damageRad, strikerMelee.m_damageRad);
            left.transform.position = enemy.ModelRef.m_leftHandBone.position;
            right.transform.position = enemy.ModelRef.m_rightHandBone.position;
        }
    }

}
