using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using EntityStates.Engi.Mine;
using EntityStates.Engi.EngiWeapon;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.Skills;

namespace Gupdate.Gameplay.Monsters
{
    public class Engi : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ENGI_SPIDERMINE_DESCRIPTION", "Place a robot mine that chases enemies for <style=cIsDamage>300% damage</style>. Can place up to 8."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Engi/EntityStates.Engi.Mine.MineArmingWeak.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(BaseMineArmingState.blastRadiusScale), 0.25f);
            };

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Engi/EntityStates.Engi.EngiWeapon.ChargeGrenades.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(ChargeGrenades.baseTotalDuration), 3 - (2 / 7));
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBubbleShield.prefab").Completed += handle =>
            {
                handle.Result.transform.Find("Collision").localScale = Vector3.one * 22f;
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/SpiderMine.prefab").Completed += handle =>
            {
                handle.Result.transform.Find("GhostAnchor").localPosition = new Vector3(0f, 0f, -0.4f);
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/SpiderMineGhost.prefab").Completed += handle =>
            {
                handle.Result.transform.Find("mdlEngiSpiderMine").localScale = Vector3.one * 0.9f;
            };
            Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiBodyPlaceSpiderMine.asset").Completed += handle =>
            {
                handle.Result.baseMaxStock = 8;
                handle.Result.baseRechargeInterval = 4f;
            };
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Engi/EntityStates.Engi.EngiWeapon.FireSpiderMine.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(FireSpiderMine.damageCoefficient), 3);
            };


            On.EntityStates.Engi.EngiWeapon.ChargeGrenades.OnEnter += ChargeGrenades_OnEnter;
            IL.EntityStates.Engi.EngiWeapon.ChargeGrenades.FixedUpdate += ChargeGrenades_FixedUpdate;
        }

        private void ChargeGrenades_OnEnter(On.EntityStates.Engi.EngiWeapon.ChargeGrenades.orig_OnEnter orig, ChargeGrenades self)
        {
            orig(self);
            self.FixedUpdate();
        }

        private void ChargeGrenades_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld<ChargeGrenades>(nameof(ChargeGrenades.maxCharges)),
                x => x.MatchCallOrCallvirt<Mathf>(nameof(Mathf.Min))
                );
            if (ilfound)
            {
                c.Emit(OpCodes.Ldc_I4, 1);
                c.Emit(OpCodes.Add);
            }
        }
    }
}
