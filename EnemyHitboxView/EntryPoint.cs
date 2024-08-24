using BepInEx;
using BepInEx.Unity.IL2CPP;
using GTFO.API;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using Enemies;
using Il2CppInterop.Runtime.Injection;
using CConsole.Interop;
using Il2CppInterop.Runtime.InteropTypes;
using static RootMotion.Demos.FBBIKSettings;

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
            ClassInjector.RegisterTypeInIl2Cpp<EnemyHitboxes>();
            ClassInjector.RegisterTypeInIl2Cpp<EnemyHitboxCuller>();

            _Harmony = new Harmony($"{VersionInfo.RootNamespace}.Harmony");
            _Harmony.PatchAll();
            Logger.Info($"Plugin has loaded with {_Harmony.GetPatchedMethods().Count()} patches!");

            CustomCommands.Register(new()
            {
                Command = "DisplayEnemyHitboxes",
                Description = "Show spheres and capsules on enemies to mark their (approximate) hitboxes. Normal hitboxes are green, weakpoints are red, and armor is grey.",
                Usage = "DisplayEnemyHitboxes <true/false>",
                Category = CConsole.Commands.CategoryType.Enemy,
                MinArgumentCount = 0
            }, 
            (in CustomCmdContext context, string[] args) =>
                {
                    if (args.Length > 0 && bool.TryParse(args[0], out bool result))
                        EnemyHitboxes.ShowHitboxes = result;
                    else
                        EnemyHitboxes.ShowHitboxes ^= true; // toggle

                    context.Log($"DisplayEnemyHitboxes: {EnemyHitboxes.ShowHitboxes}");
                }
            );
        }

        public override bool Unload()
        {
            _Harmony.UnpatchSelf();
            return base.Unload();
        }
    }

    [HarmonyPatch]
    internal class Patches
    {
        [HarmonyPatch(typeof(EnemyAgent), nameof(EnemyAgent.Setup))]
        [HarmonyPostfix]
        public static void Setup_Patch(EnemyAgent __instance)
        {
            foreach (var limb in __instance.Damage.DamageLimbs)
            {
                GameObject go = limb.gameObject;

                // This patch seems to run twice, so make sure the component is just there once so it doesn't get doubled
                // If it's trying to double up, then just cut the entire patch short since all of them always
                // (at least in testing) 
                if (go.GetComponent<EnemyHitboxes>() != null)
                    return;

                Collider collider = go.GetComponent<Collider>();
                if (collider == null)
                {
                    Logger.Warn($"Damage Limb {go.name} on enemy {__instance.EnemyDataID} ({__instance.EnemyData.name}) doesn't have a collider?");
                    continue;
                }

                EnemyHitboxes hitbox = go.AddComponent<EnemyHitboxes>();
                hitbox.Setup(limb, collider);


            }

            // Add a monobehaviour to the EnemyAgent to turn its renderers on and off 
            if (__instance.gameObject.GetComponent<EnemyHitboxCuller>() == null)
                __instance.gameObject.AddComponent<EnemyHitboxCuller>();

            foreach (IRF.InstancedRenderFeature irf in __instance.gameObject.GetComponentsInChildren<IRF.InstancedRenderFeature>())
                irf.enabled = !EnemyHitboxes.ShowHitboxes;

        }
    }

    internal class EnemyHitboxes : MonoBehaviour
    {
        public static Dictionary<eLimbDamageType, Color> TypeToColor = new() {
            { eLimbDamageType.Normal, Color.green },
            { eLimbDamageType.Weakspot, Color.red },
            { eLimbDamageType.Armor, Color.grey }
        };

        public static bool ShowHitboxes = false;

        // This is here solely for if i wanted to change the colors in UnityExplorer
        public static bool DoUpdate = true;

        public List<Renderer> m_renderers = new(); // Doubles as a list of the Hitbox GameObjects
        public Material m_material; // Used for all of the shapes in the hitbox so that it's easily uniform
        public Transform m_dummyTransform;

        private Dam_EnemyDamageLimb m_limb;
        private Collider m_collider;
        private FleshBulbs m_bulbController = null;
        private Transform HitboxRoot;

        
        public void Setup(Dam_EnemyDamageLimb limb, Collider collider)
        {
            // Cache the relevant components that we have immediately
            m_limb = limb;
            m_collider = collider;

            FleshBulbLimb fleshbulblimbComp = limb.gameObject.GetComponent<FleshBulbLimb>();
            if (fleshbulblimbComp != null)
                m_bulbController = fleshbulblimbComp.m_fleshBulbController;

            // Use homemade extension function to one-line the things. It makes me happy to do it like this
            if (collider.TryCastAtHome(out CapsuleCollider cap))
                SetupAsCapsule(cap);
            else if (collider.TryCastAtHome(out SphereCollider sc))
                SetupAsSphere(sc);
            else if (collider.TryCastAtHome(out BoxCollider bc))
                SetupAsBox(bc);
            else 
            {
                Logger.Error($"UNSUPPORTED COLLIDER TYPE: {collider.GetType()}");
                Destroy(this);
                return;
            }

            // Rename the root for name clarity
            HitboxRoot.gameObject.name = $"{m_limb.gameObject.name} - {HitboxRoot.gameObject.name}";

            // Ensure all of the objects are using the same material (really only matters for capsules, but just to keep everything uniform)
            // At the same time, disable all the colliders of the shapes we created
            foreach (Renderer renderer in m_renderers)
            {
                renderer.material = m_material;
                Destroy(renderer.gameObject.GetComponent<Collider>());
            }
        }
        public void SetupAsBox(BoxCollider bc)
        {
            Transform limbTransform = m_limb.transform;

            // Create the dummy object and position it accordingly
            m_dummyTransform = new GameObject("Hitbox Dummy Obj").transform;
            m_dummyTransform.parent = limbTransform;
            m_dummyTransform.localRotation = Quaternion.identity;
            m_dummyTransform.localPosition = bc.center;

            // Create the box and size it
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.localScale = bc.size.Multi(limbTransform.lossyScale);
            Logger.Info($"{limbTransform.lossyScale.x}, {limbTransform.lossyScale.y}, {limbTransform.lossyScale.z}");

            // Cache renderer and material for use in update
            Renderer renderer = box.GetComponent<Renderer>();
            m_material = renderer.material;
            m_renderers.Add(renderer);

            // The box doesn't have children, so it is the root itself
            box.transform.parent = m_limb.m_base.Owner.transform;
            HitboxRoot = box.transform;
        }
        public void SetupAsSphere(SphereCollider sc)
        {
            Transform limbTransform = m_limb.transform;

            // Calculate the sphere's to-be scale by using the existing object's scale
            Vector3 objScale = limbTransform.lossyScale;
            float newrad = sc.radius * 2 * Mathf.Max(objScale.x, objScale.y, objScale.z);

            // Create the dummy object and position it accordingly
            m_dummyTransform = new GameObject("Hitbox Dummy Obj").transform;
            m_dummyTransform.parent = limbTransform;
            m_dummyTransform.localPosition = sc.center;
            m_dummyTransform.localRotation = Quaternion.identity;

            // Create the sphere and size it to the correct radius
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = new Vector3(newrad, newrad, newrad);

            // Cache renderer and material for use in update
            var renderer = sphere.GetComponent<Renderer>();
            m_material = renderer.material;
            m_renderers.Add(renderer);

            // The sphere doesn't have children, so it is the root itself
            sphere.transform.parent = m_limb.m_base.Owner.transform;
            HitboxRoot = sphere.transform;
        }
        public void SetupAsCapsule(CapsuleCollider cap)
        {
            Transform limbTransform = m_limb.transform;

            // Adjust these numbers to get scale values for the cylinder
            float newrad = cap.radius * 2;
            float height = cap.height / 2;

            // Grab the limb's global scale for scaling the hitbox pieces later
            Vector3 objScale = limbTransform.lossyScale;

            // Create the hitbox's dummy object to store the collider's relative position and rotation
            m_dummyTransform = new GameObject("Hitbox Dummy Obj").transform;
            m_dummyTransform.parent = limbTransform;
            m_dummyTransform.localPosition = cap.center;

            // Rotate the dummy based on what direction the capsule collider is facing,
            // and scale the new cylinder/sphere radius and height by the corresponding parts
            switch (cap.direction)
            {
                case 0: // Towards x
                    {
                        m_dummyTransform.localRotation = Quaternion.Euler(0, 0, 90);
                        newrad *= Mathf.Max(objScale.y, objScale.z);
                        height *= objScale.x;
                        break;
                    }
                case 1: // Towards y
                    {
                        m_dummyTransform.localRotation = Quaternion.Euler(0, 0, 0);
                        newrad *= Mathf.Max(objScale.x, objScale.z);
                        height *= objScale.y;
                        break;
                    }
                case 2: // Towards z
                    {
                        m_dummyTransform.localRotation = Quaternion.Euler(90, 0, 0);
                        newrad *= Mathf.Max(objScale.x, objScale.y);
                        height *= objScale.z;
                        break;
                    }
            }
            // This makes the cylinder only tall enough to actually contain the cylindrical part of the capsule
            // It bottoms out at zero for hopefully obvious reasons
            height = Mathf.Max(0, height - newrad / 2);

            // This part copies the position and rotation of the dummy object above
            // This is to avoid stacked scaling schenanigans
            HitboxRoot = new GameObject("Capsule").transform;
            
            // Parent the root to the EnemyAgent object so it will be destroyed when the enemy is
            HitboxRoot.parent = m_limb.m_base.Owner.transform;

            // Now create the three shape parts of the capsule and parent them to the root
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.parent = HitboxRoot.transform;
            cylinder.transform.localRotation = Quaternion.identity;
            cylinder.transform.localPosition = Vector3.zero;
            cylinder.transform.localScale = new Vector3(newrad, height, newrad);

            GameObject capsuleTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            capsuleTop.name = "CapsuleTop";
            capsuleTop.transform.parent = HitboxRoot.transform;
            capsuleTop.transform.localRotation = Quaternion.identity;
            capsuleTop.transform.localPosition = new Vector3(0, height, 0);
            capsuleTop.transform.localScale = new Vector3(newrad, newrad, newrad);

            GameObject capsuleBottom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            capsuleBottom.name = "CapsuleBottom";
            capsuleBottom.transform.parent = HitboxRoot.transform;
            capsuleBottom.transform.localRotation = Quaternion.identity;
            capsuleBottom.transform.localPosition = new Vector3(0, -height, 0);
            capsuleBottom.transform.localScale = new Vector3(newrad, newrad, newrad);

            // cache the material and renderer itself. The material because it's changed based on limb type in `Update`,
            // and the renderer itself for toggling on and off
            Renderer cyl_renderer = cylinder.GetComponent<Renderer>();
            m_material = cyl_renderer.material;
            
            m_renderers.Add(cyl_renderer);
            m_renderers.Add(capsuleTop.GetComponent<Renderer>());
            m_renderers.Add(capsuleBottom.GetComponent<Renderer>());
        }

        public void OnDisable()
        {
            // Make sure the hitboxes get disabled when the gameobject does
            // since the collider won't be hittable anymore
            // Is an issue for moms and such, where destroying the bubble disables the GO
            // so update won't run again to turn it off
            foreach (Renderer renderer in m_renderers)
            {
                renderer.enabled = false;
                renderer.gameObject.SetActive(false);
            }
        }

        public void Update()
        {
            // DoUpdate var is pretty much just for if you want to manually change the colors with UnityExplorer
            // Otherwise, just don't do the update if the enemy is culled
            if (!DoUpdate || !m_limb.m_base.Owner.MovingCuller.IsShown) return;

            // Set the color to the type of limb
            // This was made into a monobehaviour initially for this, as EEC enemies set the limb type late enough
            // that I couldn't just assign the right type in a patch
            // It also happened to work out nice for big flyers in vanilla which changes the eye's limb type
            // based on whether or not you should be able to shoot it
            m_material.color = TypeToColor[m_limb.m_type];

            // Show/Hide the hitbox parts
            foreach (Renderer renderer in m_renderers)
            {
                renderer.enabled = ShowHitboxes;
                GameObject gameObject = renderer.gameObject;
                gameObject.SetActive(m_collider.enabled && ShowHitboxes);
            }

            // Force enable/disable the visual tumor bubbles
            if (m_bulbController != null)
                m_bulbController.ForceRenderingOff = ShowHitboxes;

            // Absolute positions
            HitboxRoot.position = m_dummyTransform.position;
            HitboxRoot.rotation = m_dummyTransform.rotation;


        }
    }

    internal class EnemyHitboxCuller : MonoBehaviour
    {
        EnemyAgent m_agent;
        IRF.InstancedRenderFeature[] m_allIRFs;
        public void Start()
        {
            m_agent = GetComponent<EnemyAgent>();
            m_allIRFs = GetComponentsInChildren<IRF.InstancedRenderFeature>();
        }
        public void LateUpdate()
        {
            foreach (var renderer in m_agent.MovingCuller.Culler.Renderers)
                renderer.enabled = !EnemyHitboxes.ShowHitboxes;

            foreach (IRF.InstancedRenderFeature irf in m_allIRFs)
                irf.enabled = !EnemyHitboxes.ShowHitboxes;
        }
    }

    static class Extension
    {
        public static bool TryCastAtHome<T, O>(this T InComp, out O OutComp)
            where T : Il2CppObjectBase
            where O : Il2CppObjectBase
        {
            OutComp = InComp.TryCast<O>();
            return OutComp != null;
        }

        public static Vector3 Multi(this Vector3 arg1, Vector3 arg2)
        {
            arg1.x *= arg2.x;
            arg1.y *= arg2.y;
            arg1.z *= arg2.z;
            return arg1;
        }
    }
}