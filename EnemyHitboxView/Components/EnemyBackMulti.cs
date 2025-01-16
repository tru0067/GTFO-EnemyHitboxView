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
    // Made by tru0067
    internal class EnemyBackMulti : MonoBehaviour
    {
        public static bool ShowVectors = false;

        public static Vector3 floorOffset = Vector3.up;  // Offset vector so everything doesn't end up displaying in the floor.

        public EnemyAgent enemy;
        public LineRenderer forward;
        public LineRenderer chest;

        public void Start()
        {
            GameObject forwardGO = new();
            forward = forwardGO.AddComponent<LineRenderer>();
            forward.material.color = Color.green;
            forward.SetWidth(0.1f, 0.1f);
            GameObject chestGO = new();
            chest = chestGO.AddComponent<LineRenderer>();
            chest.material.color = Color.blue;
            chest.SetWidth(0.1f, 0.1f);
        }

        public void OnDestroy()
        {
            if (forward != null)
                forward.gameObject.Destroy();
            if (chest != null)
                chest.gameObject.Destroy();
        }

        public void Update()
        {
            if (enemy == null || forward == null || chest == null)
                return;

            if (!ShowVectors || !enemy.Alive || !enemy.EnemyBalancingData.AllowDamgeBonusFromBehind)
            {
                forward.positionCount = 0;
                chest.positionCount = 0;
                forward.enabled = false;
                chest.enabled = false;
                return;
            }

            forward.enabled = true;
            // Grab the relevant vector from the enemy.
            Vector3 forwardVector = enemy.Forward;
            // Use it to set up our lines.
            forward.positionCount = 2;
            forward.SetPosition(0, enemy.Position + floorOffset);
            forward.SetPosition(1, enemy.Position + floorOffset + forwardVector);
            // Repeat for chest (if it exists).
            if (enemy.ModelRef.m_chestBone != null)
            {
                chest.enabled = true;
                Vector3 chestVector = enemy.ModelRef.m_chestBone.up * -1f;
                chest.positionCount = 2;
                chest.SetPosition(0, enemy.Position + floorOffset);
                chest.SetPosition(1, enemy.Position + floorOffset + chestVector);
            }
        }
    }
}
