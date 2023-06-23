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

namespace Gupdate.Gameplay.Monsters
{
    public class Bandit : ModBehaviour
    {
        /*public override (string, string)[] GetLang() => new[]
        {
            ("MAGE_SPECIAL_FIRE_DESCRIPTION", "<style=cIsDamage>Ignite</style>. Burn all enemies in front of you for <style=cIsDamage>2400% damage</style>."),
        };*/

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

            //On.EntityStates.Bandit2.Weapon.FireShotgun2.ModifyBullet += FireShotgun2_ModifyBullet;
        }

        /*private void FireShotgun2_ModifyBullet(On.EntityStates.Bandit2.Weapon.FireShotgun2.orig_ModifyBullet orig, FireShotgun2 self, BulletAttack bulletAttack)
        {
            orig(self, bulletAttack);
            bulletAttack.falloffModel = BulletAttack.FalloffModel.Buckshot;
        }*/
    }
}
