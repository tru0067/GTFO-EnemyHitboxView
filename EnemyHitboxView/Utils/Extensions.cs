using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnemyHitboxView.Utils
{
    static class Extensions
    {
        public static bool TryCastAtHome<T, O>(this T InComp, out O OutComp)
            where T : Il2CppObjectBase
            where O : Il2CppObjectBase
        {
            OutComp = InComp.TryCast<O>();
            return OutComp != null;
        }

        // Multiply all of the components of the input vector by the corresponding components of the argument vector
        public static Vector3 Multiply(this Vector3 arg1, Vector3 arg2)
        {
            arg1.x *= arg2.x;
            arg1.y *= arg2.y;
            arg1.z *= arg2.z;
            return arg1;
        }
    }
}
