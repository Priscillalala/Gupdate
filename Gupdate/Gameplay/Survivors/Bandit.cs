using System;
using BepInEx;
using EntityStates.Bandit2.Weapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Skills;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Gupdate.Gameplay.Monsters
{
    public class Bandit : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("BANDIT2_SECONDARY_ALT_DESCRIPTION", "Throw a hidden blade for <style=cIsDamage>140% damage</style>. Critical Strikes also cause <style=cIsHealth>hemorrhaging</style>. Can hold up to 2."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Bandit2/EntityStates.Bandit2.Weapon.Bandit2FireRifle.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(Bandit2FireRifle.spreadBloomValue), 0.5f);
            };
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Bandit2/EntityStates.Bandit2.Weapon.SlashBlade.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(SlashBlade.forceForwardVelocity), true);
                handle.Result.TryModifyFieldValue(nameof(SlashBlade.forwardVelocityCurve), AnimationCurve.EaseInOut(0f, 0.15f, 1f, 0f));
            };
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Bandit2/EntityStates.Bandit2.Weapon.Bandit2FireShiv.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(Bandit2FireShiv.damageCoefficient), 1.4f);
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ShivProjectile.prefab").Completed += handle =>
            {
                SetRandomRotation setRandomRotation = handle.Result.GetComponentInChildren<SetRandomRotation>();
                if (setRandomRotation)
                {
                    DestroyImmediate(setRandomRotation);
                }
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2ThrowShiv.prefab").Completed += handle =>
            {
                if (handle.Result.transform.TryFind("SwingTrail", out Transform swingTrail) && swingTrail.TryGetComponent(out ParticleSystem particleSystem))
                {
                    var main = particleSystem.main;
                    main.startRotationX = new ParticleSystem.MinMaxCurve(0f);
                    main.startRotationY = new ParticleSystem.MinMaxCurve(0f);
                    main.startRotationZ = new ParticleSystem.MinMaxCurve(225f);
                }
            };

            Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Bandit2/Bandit2SerratedShivs.asset").Completed += handle =>
            {
                handle.Result.baseMaxStock = 2;
            };

            Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Bandit2/ThrowSmokebomb.asset").Completed += handle =>
            {
                handle.Result.baseRechargeInterval = 8f;
            };

            IL.EntityStates.Bandit2.Weapon.Bandit2FireShiv.FireShiv += Bandit2FireShiv_FireShiv;
            //On.EntityStates.Bandit2.Weapon.FireShotgun2.ModifyBullet += FireShotgun2_ModifyBullet;
        }

        private void Bandit2FireShiv_FireShiv(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (ilfound = c.TryGotoNext(MoveType.Before, x => x.MatchStfld<FireProjectileInfo>(nameof(FireProjectileInfo.rotation))))
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<Quaternion, Bandit2FireShiv, Quaternion>>((origRotation, fireShiv) =>
                {
                    Ray aimRay = fireShiv.GetAimRay();
                    if (fireShiv.projectilePrefab && Util.CharacterRaycast(fireShiv.gameObject, aimRay, out RaycastHit hitInfo, 40f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal)
                    && fireShiv.projectilePrefab.TryGetComponent(out ProjectileSimple projectileSimple) && fireShiv.projectilePrefab.TryGetComponent(out Rigidbody rigidbody) && rigidbody.useGravity)
                    {
                        float projectileBaseSpeed = projectileSimple.desiredForwardSpeed;
                        Vector3 vector = hitInfo.point - aimRay.origin;
                        Vector2 normalized = new Vector2(vector.x, vector.z);
                        float magnitude = normalized.magnitude;
                        float y = Trajectory.CalculateInitialYSpeed(magnitude / projectileBaseSpeed, vector.y);
                        Vector3 a = new Vector3(normalized.x / magnitude * projectileBaseSpeed, y, normalized.y / magnitude * projectileBaseSpeed);
                        //dest.speedOverride = a.magnitude;
                        //Debug.LogWarning("Expected speed: " + a.magnitude);
                        return Util.QuaternionSafeLookRotation(a);
                    }
                    return origRotation;
                });
            }
        }

        /*private void FireShotgun2_ModifyBullet(On.EntityStates.Bandit2.Weapon.FireShotgun2.orig_ModifyBullet orig, FireShotgun2 self, BulletAttack bulletAttack)
        {
            orig(self, bulletAttack);
            bulletAttack.falloffModel = BulletAttack.FalloffModel.Buckshot;
        }*/
    }
}
