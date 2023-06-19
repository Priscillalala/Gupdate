using System;
using BepInEx;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Items;

namespace Gupdate.Gameplay.Items
{
    public class IgnitionTank : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_STRENGTHENBURN_PICKUP", "Ignite effects deal double damage. All explosions have a chance to ignite enemies."),
            ("ITEM_STRENGTHENBURN_DESC", "Ignite effects deal <style=cIsDamage>100%</style> <style=cStack>(+100% per stack)</style> more damage over time. <style=cIsDamage>50%</style> chance for explosive attacks to <style=cIsDamage>ignite</style> enemies."),
        };

        public void Awake()
        {
            On.RoR2.BlastAttack.Fire += BlastAttack_Fire;
            IL.RoR2.StrengthenBurnUtils.CheckDotForUpgrade += StrengthenBurnUtils_CheckDotForUpgrade;
        }

        private void StrengthenBurnUtils_CheckDotForUpgrade(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(DLC1Content), nameof(DLC1Content.Items.StrengthenBurn), out int locStackIndex))
            {
                int locMultiplierIndex = -1;
                ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcI4(1),
                x => x.MatchLdcI4(out _),
                x => x.MatchLdloc(locStackIndex),
                x => x.MatchMul(),
                x => x.MatchAdd(),
                x => x.MatchConvR4(),
                x => x.MatchStloc(out locMultiplierIndex)
                );

                if (ilfound)
                {
                    c.Next.Operand = 1;
                }

                ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdflda<InflictDotInfo>(nameof(InflictDotInfo.totalDamage))
                ) && c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(locMultiplierIndex)
                );

                if (ilfound)
                {
                    c.EmitDelegate<Func<float, float>>(Mathf.Sqrt);
                }
            }
        }

        private BlastAttack.Result BlastAttack_Fire(On.RoR2.BlastAttack.orig_Fire orig, BlastAttack self)
        {
            bool modified = false;
            if (self.attacker && self.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.HasItem(DLC1Content.Items.StrengthenBurn) 
                && (self.damageType & DamageType.IgniteOnHit) == 0 && Util.CheckRoll(50f, attackerBody.master))
            {
                self.damageType |= DamageType.IgniteOnHit;
                modified = true;
                EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, new EffectData
                {
                    origin = self.position,
                    scale = self.radius,
                    rotation = Util.QuaternionSafeLookRotation(self.bonusForce)
                }, true);
            }
            BlastAttack.Result result = orig(self);
            if (modified)
            {
                self.damageType &= ~DamageType.IgniteOnHit;
            }
            return result;
        }
    }
}
