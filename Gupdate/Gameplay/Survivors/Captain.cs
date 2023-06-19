using System;
using BepInEx;
using EntityStates.FlyingVermin.Weapon;
using EntityStates.Vermin.Weapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using EntityStates.Captain.Weapon;
using System.Collections.Generic;
using EntityStates.CaptainSupplyDrop;

namespace Gupdate.Gameplay.Monsters
{
    public class Captain : ModBehaviour
    {
        /*public override (string, string)[] GetLang() => new[]
        {
            ("CROCO_PASSIVE_ALT_DESCRIPTION", "Attacks that apply <style=cIsHealing>Poison</style> apply stacking <style=cIsDamage>Blight</style> instead, dealing <style=cIsDamage>60% damage per second</style> and reducing armor by <style=cIsDamage>5</style>."),
        };*/

        private static readonly Dictionary<HackingInProgressState, int> hackToBaseCostServer = new Dictionary<HackingInProgressState, int>();

        public void Awake()
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Captain/EntityStates.CaptainSupplyDrop.HackingInProgressState.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(HackingInProgressState.baseDuration), 4.5f);
            };

            On.EntityStates.CaptainSupplyDrop.HackingInProgressState.OnEnter += HackingInProgressState_OnEnter;
            On.EntityStates.CaptainSupplyDrop.HackingInProgressState.FixedUpdate += HackingInProgressState_FixedUpdate;
            On.EntityStates.CaptainSupplyDrop.HackingInProgressState.OnExit += HackingInProgressState_OnExit;
            On.EntityStates.Captain.Weapon.FireCaptainShotgun.ModifyBullet += FireCaptainShotgun_ModifyBullet;
        }

        private void HackingInProgressState_OnEnter(On.EntityStates.CaptainSupplyDrop.HackingInProgressState.orig_OnEnter orig, HackingInProgressState self)
        {
            if (NetworkServer.active && self.target)
            {
                hackToBaseCostServer[self] = self.target.cost;
            }
            orig(self);
        }

        private void HackingInProgressState_FixedUpdate(On.EntityStates.CaptainSupplyDrop.HackingInProgressState.orig_FixedUpdate orig, HackingInProgressState self)
        {
            if (NetworkServer.active && self.target && hackToBaseCostServer.TryGetValue(self, out int baseCost) && Util.CheckRoll(Time.fixedDeltaTime * 180f, self.GetComponent<GenericOwnership>()?.ownerObject?.GetComponent<CharacterBody>()?.master))
            {
                self.target.Networkcost = Mathf.CeilToInt((1f - self.energyComponent.normalizedEnergy) * baseCost);
            }
            orig(self);
        }

        private void HackingInProgressState_OnExit(On.EntityStates.CaptainSupplyDrop.HackingInProgressState.orig_OnExit orig, HackingInProgressState self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                hackToBaseCostServer.Remove(self);
            }
        }

        private void FireCaptainShotgun_ModifyBullet(On.EntityStates.Captain.Weapon.FireCaptainShotgun.orig_ModifyBullet orig, FireCaptainShotgun self, BulletAttack bulletAttack)
        {
            orig(self, bulletAttack);
            if (self.characterBody.spreadBloomAngle <= FireCaptainShotgun.tightSoundSwitchThreshold)
            {
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            }
        }
    }
}
