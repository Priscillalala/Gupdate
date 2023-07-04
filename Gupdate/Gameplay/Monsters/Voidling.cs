using System;
using BepInEx;
using RoR2.CharacterAI;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine.Events;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using Mono.Cecil;
using RoR2.Skills;
using EntityStates;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Gupdate.Gameplay.Monsters
{
    public class Voidling : ModBehaviour
    {
        private static GameObject voidRaidCrabCollisionPrefab;
        public void Awake()
        {
            Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidRaidCrab/matVoidRaidCrabBrain.mat").Completed += handle =>
            {
                handle.Result.SetTextureScale("_MainTex", new Vector2(3f, 4.5f));
                handle.Result.SetTextureOffset("_MainTex", new Vector2(0f, -2.4f));
                handle.Result.SetFloat("_NormalStrength", 2f);
                handle.Result.SetTexture("_NormalTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/blackbeach/texBBMudNormal.png").WaitForCompletion());
                handle.Result.SetInt("_RampInfo", 1);
            };

            Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabVacuumAttack.asset").Completed += handle =>
            {
                handle.Result.baseRechargeInterval = 4f;
            };

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/DLC1/VoidRaidCrab/EntityStates.VoidRaidCrab.SpinBeamAttack.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(SpinBeamAttack.revolutionsCurve), AnimationCurve.EaseInOut(0f, 0f, 1f, 1.15f));
                handle.Result.TryModifyFieldValue(nameof(SpinBeamAttack.baseDuration), 4f);
            };

            GameObject miniVoidRaidCrabPhase1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
            CharacterBody miniVoidRaidCrabPhase1Body = miniVoidRaidCrabPhase1.GetComponent<CharacterBody>();
            miniVoidRaidCrabPhase1Body.baseMoveSpeed = 45f;
            miniVoidRaidCrabPhase1Body.baseAcceleration = 45f;
            GameObject voidRaidCrab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabBody.prefab").WaitForCompletion();
            Transform mdlMiniVoidRaidCrab = miniVoidRaidCrabPhase1.GetComponent<ModelLocator>().modelTransform;
            Transform mdlVoidRaidCrab = voidRaidCrab.GetComponent<ModelLocator>().modelTransform;
            
            SphereCollider mainCollider = miniVoidRaidCrabPhase1.GetComponent<SphereCollider>();
            MeshCollider standableCollider = mdlMiniVoidRaidCrab.transform.Find("VoidRaidCrabArmature/ROOT/HeadBase/StandableSurfacePosition/StandableSurface").GetComponent<MeshCollider>();
            
            VoidRaidCrabLegController legController = mdlMiniVoidRaidCrab.gameObject.AddComponent<VoidRaidCrabLegController>();
            legController.coreColliders = new Collider[] { mainCollider, standableCollider };
            StriderLegController striderLegController = mdlMiniVoidRaidCrab.GetComponent<StriderLegController>();
            legController.centerOfGravity = striderLegController.centerOfGravity;
            legController.feet = striderLegController.feet;
            legController.footDampTime = striderLegController.footDampTime;
            legController.footMoveString = striderLegController.footMoveString;
            legController.footPlantEffect = striderLegController.footPlantEffect;
            legController.footPlantString = striderLegController.footPlantString;
            legController.footRaycastDirection = striderLegController.footRaycastDirection;
            legController.footRaycastFrequency = striderLegController.footRaycastFrequency;
            legController.lerpCurve = striderLegController.lerpCurve;
            legController.maxFeetReplantingAtOnce = striderLegController.maxFeetReplantingAtOnce;
            legController.maxRaycastDistance = striderLegController.maxRaycastDistance;
            legController.overstepDistance = 14f;
            legController.raycastVerticalOffset = striderLegController.raycastVerticalOffset;
            legController.replantDuration = 0.5f;
            legController.replantHeight = striderLegController.replantHeight;
            legController.stabilityRadius = 20f;
            legController.stompRadius = 20f;
            legController.stompDamageCoefficient = 4f;
            legController.stompProcCoefficient = 1f;
            legController.baseStompForce = 500f;
            legController.bonusStompForce = Vector3.up * 500f;
            legController.stompFalloffModel = BlastAttack.FalloffModel.Linear;
            DestroyImmediate(striderLegController);

            foreach (MeshCollider collider in mdlVoidRaidCrab.transform.Find("VoidRaidCrabArmature/ROOT/LegBase").GetComponentsInChildren<MeshCollider>())
            {
                string path = Util.BuildPrefabTransformPath(mdlVoidRaidCrab, collider.transform.parent);
                LogInfo(path);
                Transform other = mdlMiniVoidRaidCrab.transform.Find(path);
                if (other)
                {
                    LogWarning("Found!");
                    VoidRaidCrabCollisionProxy proxy = other.gameObject.AddComponent<VoidRaidCrabCollisionProxy>();
                    proxy.position = collider.transform.localPosition;
                    proxy.rotation = collider.transform.localRotation;
                    proxy.scale = collider.transform.localScale;
                    proxy.mesh = collider.sharedMesh;
                    proxy.legController = legController;
                }
            }

            voidRaidCrabCollisionPrefab = new GameObject().InstantiateClone("VoidRaidCrabCollision", false);
            voidRaidCrabCollisionPrefab.layer = 11;

            Rigidbody rigidbody = voidRaidCrabCollisionPrefab.AddComponent<Rigidbody>();
            rigidbody.mass = 1;
            rigidbody.drag = 0;
            rigidbody.angularDrag = 0;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            PhysicsMover physicsMover = voidRaidCrabCollisionPrefab.AddComponent<PhysicsMover>();
            physicsMover.Rigidbody = rigidbody;

            BasicPhysicsMoverController physicsMoverController = voidRaidCrabCollisionPrefab.AddComponent<BasicPhysicsMoverController>();
            physicsMoverController.Mover = physicsMover;

            physicsMover.MoverController = physicsMoverController;

            MeshCollider meshCollider = voidRaidCrabCollisionPrefab.AddComponent<MeshCollider>();
            meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase;
            meshCollider.convex = false;
            meshCollider.isTrigger = false;
            SurfaceDefProvider surfaceDefProvider = voidRaidCrabCollisionPrefab.AddComponent<SurfaceDefProvider>();
            surfaceDefProvider.surfaceDef = Addressables.LoadAssetAsync<SurfaceDef>("RoR2/DLC1/VoidRaidCrab/sdVoidRaidCrabWalkableSurface.asset").WaitForCompletion();

            /*Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabSpinBeam.asset").Completed += handle =>
            {
                handle.Result.baseRechargeInterval = 12f;
                handle.Result.baseMaxStock = 2;
                handle.Result.activationState = new SerializableEntityStateType(typeof(ChargeGravityBump));
            };*/

            On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.OnEnter += BaseSpinBeamAttackState_OnEnter;
            On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.OnExit += BaseSpinBeamAttackState_OnExit;
            On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.GetBeamRay += BaseSpinBeamAttackState_GetBeamRay;
            On.EntityStates.VoidRaidCrab.SpinBeamExit.OnEnter += SpinBeamExit_OnEnter;
            On.EntityStates.VoidRaidCrab.VacuumExit.OnEnter += VacuumExit_OnEnter;
            IL.EntityStates.VoidRaidCrab.SpinBeamAttack.FireBeamBulletAuthority += SpinBeamAttack_FireBeamBulletAuthority;
        }

        private void BaseSpinBeamAttackState_OnEnter(On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.orig_OnEnter orig, BaseSpinBeamAttackState self)
        {
            orig(self);
            self.characterBody.aimOriginTransform = self.muzzleTransform;
        }

        private void BaseSpinBeamAttackState_OnExit(On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.orig_OnExit orig, BaseSpinBeamAttackState self)
        {
            self.characterBody.aimOriginTransform = self.modelLocator.modelBaseTransform.Find("AimOrigin");
            orig(self);
        }

        private Ray BaseSpinBeamAttackState_GetBeamRay(On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.orig_GetBeamRay orig, BaseSpinBeamAttackState self)
        {
            Ray aimRay = self.GetAimRay();
            Vector3 desired = aimRay.direction;
            Vector3 forward = self.headTransform.forward;

            float yCoefficient = 1f - Mathf.Clamp01(Vector2.Angle(new Vector2(forward.x, forward.z), new Vector2(desired.x, desired.z)) / 180f);
            float y = Mathf.Lerp(desired.y * -0.5f, desired.y, yCoefficient);

            return new Ray(aimRay.origin, new Vector3(forward.x, y, forward.z));
        }

        private void SpinBeamExit_OnEnter(On.EntityStates.VoidRaidCrab.SpinBeamExit.orig_OnEnter orig, SpinBeamExit self)
        {
            orig(self);
            GenericSkill genericSkill = self.skillLocator?.utility;
            if (genericSkill)
            {
                genericSkill.SetSkillOverride(self.outer,
                    Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabVacuumAttack.asset").WaitForCompletion(),
                    GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        private void VacuumExit_OnEnter(On.EntityStates.VoidRaidCrab.VacuumExit.orig_OnEnter orig, VacuumExit self)
        {
            orig(self);
            GenericSkill genericSkill = self.skillLocator?.utility;
            if (genericSkill)
            {
                genericSkill.UnsetSkillOverride(self.outer,
                    Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabVacuumAttack.asset").WaitForCompletion(),
                    GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        private void SpinBeamAttack_FireBeamBulletAuthority(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (ilfound = c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt<BulletAttack>(nameof(BulletAttack.Fire))))
            {
                c.EmitDelegate<Func<BulletAttack, BulletAttack>>(bulletAttack =>
                {
                    bulletAttack.damageType |= DamageType.NonLethal | DamageType.BypassArmor | DamageType.BypassBlock;
                    return bulletAttack;
                });
            }
        }

        public class VoidRaidCrabLegController : MonoBehaviour
        {
            public Collider[] coreColliders;
            private readonly HashSet<Collider> legColliders = new HashSet<Collider>();

            public Transform centerOfGravity;
            public StriderLegController.FootInfo[] feet;
            public Vector3 footRaycastDirection;
            public float raycastVerticalOffset;
            public float maxRaycastDistance;
            public float footDampTime;
            public float stabilityRadius;
            public float replantDuration;
            public float replantHeight;
            public float overstepDistance;
            public AnimationCurve lerpCurve;
            public GameObject footPlantEffect;
            public string footPlantString;
            public string footMoveString;
            public float footRaycastFrequency = 0.2f;
            public int maxFeetReplantingAtOnce = 9999;

            public bool stompOnLegPlant = true;
            public float stompDamageCoefficient;
            public float stompProcCoefficient;
            public float stompRadius;
            public float baseStompForce;
            public Vector3 bonusStompForce;
            public BlastAttack.FalloffModel stompFalloffModel;
            public DamageType stompDamageType = DamageType.Generic;

            private CharacterModel characterModel;

            public void AddLegCollider(Collider legCollider)
            {
                foreach (Collider collider in coreColliders)
                {
                    Physics.IgnoreCollision(legCollider, collider);
                }
                legColliders.Add(legCollider);
            }

            public Vector3 GetCenterOfStance()
            {
                Vector3 a = Vector3.zero;
                for (int i = 0; i < feet.Length; i++)
                {
                    a += feet[i].transform.position;
                }
                return a / (float)feet.Length;
            }

            public void Awake()
            {
                characterModel = base.GetComponent<CharacterModel>();
                for (int i = 0; i < feet.Length; i++)
                {
                    feet[i].footState = StriderLegController.FootState.Planted;
                    feet[i].plantPosition = feet[i].referenceTransform.position;
                    feet[i].trailingTargetPosition = feet[i].plantPosition;
                    feet[i].footRaycastTimer = UnityEngine.Random.Range(0f, 1f / footRaycastFrequency);
                }
            }

            public void Update()
            {
                int num = 0;
                int num2 = 0;
                for (int i = 0; i < feet.Length; i++)
                {
                    if (feet[i].footState == StriderLegController.FootState.Replanting)
                    {
                        num2++;
                    }
                }
                for (int j = 0; j < feet.Length; j++)
                {
                    StriderLegController.FootInfo[] array = feet;
                    int num3 = j;
                    array[num3].footRaycastTimer = array[num3].footRaycastTimer - Time.deltaTime;
                    Transform footTransform = feet[j].transform;
                    Transform referenceTransform = feet[j].referenceTransform;
                    Vector3 vector = Vector3.zero;
                    float num4 = 0f;
                    StriderLegController.FootState footState = feet[j].footState;
                    if (footState != StriderLegController.FootState.Planted)
                    {
                        if (footState == StriderLegController.FootState.Replanting)
                        {
                            StriderLegController.FootInfo[] array2 = feet;
                            int num5 = j;
                            array2[num5].stopwatch = array2[num5].stopwatch + Time.deltaTime;
                            Vector3 plantPosition = feet[j].plantPosition;
                            Vector3 vector2 = referenceTransform.position;
                            vector2 += Vector3.ProjectOnPlane(vector2 - plantPosition, Vector3.up).normalized * overstepDistance;
                            float num6 = lerpCurve.Evaluate(feet[j].stopwatch / replantDuration);
                            vector = Vector3.Lerp(plantPosition, vector2, num6);
                            num4 = Mathf.Sin(num6 * 3.1415927f) * replantHeight;
                            if (feet[j].stopwatch >= replantDuration)
                            {
                                feet[j].plantPosition = vector2;
                                feet[j].stopwatch = 0f;
                                feet[j].footState = StriderLegController.FootState.Planted;
                                OnLegPlanted(footTransform, vector2);
                            }
                        }
                    }
                    else
                    {
                        num++;
                        vector = feet[j].plantPosition;
                        if ((referenceTransform.position - vector).sqrMagnitude > stabilityRadius * stabilityRadius && num2 < maxFeetReplantingAtOnce)
                        {
                            feet[j].footState = StriderLegController.FootState.Replanting;
                            Util.PlaySound(footMoveString, footTransform.gameObject);
                            num2++;
                        }
                    }
                    Ray ray = default;
                    ray.direction = footTransform.TransformDirection(footRaycastDirection.normalized);
                    ray.origin = vector - ray.direction * raycastVerticalOffset;
                    if (feet[j].footRaycastTimer <= 0f)
                    {
                        feet[j].footRaycastTimer = 1f / footRaycastFrequency;
                        feet[j].lastYOffsetFromRaycast = feet[j].currentYOffsetFromRaycast;
                        if (SafeRaycast(ray, out RaycastHit raycastHit, maxRaycastDistance + raycastVerticalOffset))
                        {
                            feet[j].currentYOffsetFromRaycast = raycastHit.point.y - vector.y;
                        }
                        else
                        {
                            feet[j].currentYOffsetFromRaycast = 0f;
                        }
                    }
                    float num7 = Mathf.Lerp(feet[j].currentYOffsetFromRaycast, feet[j].lastYOffsetFromRaycast, feet[j].footRaycastTimer / (1f / footRaycastFrequency));
                    vector.y += num4 + num7;
                    feet[j].trailingTargetPosition = Vector3.SmoothDamp(feet[j].trailingTargetPosition, vector, ref feet[j].velocity, footDampTime);
                    footTransform.position = feet[j].trailingTargetPosition;
                }
            }

            public Vector3 GetArcPosition(Vector3 start, Vector3 end, float arcHeight, float t)
            {
                return Vector3.Lerp(start, end, Mathf.Sin(t * 3.1415927f * 0.5f)) + new Vector3(0f, Mathf.Sin(t * 3.1415927f) * arcHeight, 0f);
            }

            public bool SafeRaycast(Ray ray, out RaycastHit hitInfo, float distance)
            {
                RaycastHit[] raycastHits = Physics.RaycastAll(ray, distance, LayerIndex.world.mask);
                for (int i = 0; i < raycastHits.Length; i++)
                {
                    if (!legColliders.Contains(raycastHits[i].collider))
                    {
                        hitInfo = raycastHits[i];
                        return true;
                    }
                }
                hitInfo = default;
                return false;
            }

            public void OnLegPlanted(Transform footTransform, Vector3 plantPosition)
            {
                Util.PlaySound(footPlantString, footTransform.gameObject);
                if (footPlantEffect)
                {
                    EffectManager.SimpleEffect(footPlantEffect, plantPosition, Quaternion.identity, false);
                }
                if (stompOnLegPlant && characterModel?.body && Util.HasEffectiveAuthority(characterModel.body.gameObject))
                {
                    new BlastAttack
                    {
                        attacker = characterModel.body.gameObject,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        baseDamage = characterModel.body.damage * stompDamageCoefficient,
                        baseForce = baseStompForce,
                        bonusForce = bonusStompForce,
                        crit = characterModel.body.RollCrit(),
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = stompDamageType,
                        falloffModel = stompFalloffModel,
                        inflictor = footTransform.gameObject,
                        position = plantPosition,
                        procChainMask = default,
                        procCoefficient = stompProcCoefficient,
                        radius = stompRadius,
                        teamIndex = characterModel.body.teamComponent.teamIndex
                    }.Fire();
                }
            }
        }

        public class VoidRaidCrabCollisionProxy : MonoBehaviour
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
            public Mesh mesh;
            public VoidRaidCrabLegController legController;

            public void Awake()
            {
                Transform reference = new GameObject("Collider Reference Position").transform;
                reference.SetParent(base.transform);
                reference.localPosition = position;
                reference.localRotation = rotation;

                MeshCollider collider = Instantiate(voidRaidCrabCollisionPrefab, Vector3.zero, Quaternion.identity, base.transform).GetComponent<MeshCollider>();
                //collider.transform.localPosition = position;
                //collider.transform.localRotation = rotation;
                collider.transform.localScale = scale;
                collider.sharedMesh = mesh;
                collider.GetComponent<BasicPhysicsMoverController>().referenceTransformPosition = reference;

                legController.AddLegCollider(collider);

                Destroy(this);
            }
        }
    }
}

/*GameObject collision = new GameObject("Collision");
collision.transform.SetParent(base.transform);
collision.layer = 11;
collision.transform.localPosition = position;
collision.transform.localRotation = rotation;
collision.transform.localScale = scale;

Rigidbody rigidbody = collision.AddComponent<Rigidbody>();
rigidbody.mass = 1;
rigidbody.drag = 0;
rigidbody.angularDrag = 0;
rigidbody.useGravity = false;
rigidbody.isKinematic = true;
rigidbody.interpolation = RigidbodyInterpolation.None;
rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

PhysicsMover physicsMover = collision.AddComponent<PhysicsMover>();
physicsMover.Rigidbody = rigidbody;

BasicPhysicsMoverController physicsMoverController = collision.AddComponent<BasicPhysicsMoverController>();
physicsMoverController.referenceTransformPosition = base.transform;
physicsMoverController.Mover = physicsMover;

physicsMover.MoverController = physicsMoverController;

MeshCollider meshCollider = collision.AddComponent<MeshCollider>();
meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase;
meshCollider.sharedMesh = mesh;
meshCollider.convex = false;
meshCollider.isTrigger = false;
SurfaceDefProvider surfaceDefProvider = collision.AddComponent<SurfaceDefProvider>();
surfaceDefProvider.surfaceDef = Addressables.LoadAssetAsync<SurfaceDef>("RoR2/DLC1/VoidRaidCrab/sdVoidRaidCrabWalkableSurface.asset").WaitForCompletion();
/*DisableCollisionsBetweenColliders disableCollisionsBetweenColliders = collision.AddComponent<DisableCollisionsBetweenColliders>();
disableCollisionsBetweenColliders.collidersA = new[] { meshCollider };
disableCollisionsBetweenColliders.collidersB = new[] { main };*/
//Physics.IgnoreCollision(meshCollider, main);*/
