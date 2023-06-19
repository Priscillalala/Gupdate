using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Gupdate.Gameplay
{
    public class Burn : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/FireTrail.prefab").Completed += handle =>
            {
                handle.Result.GetComponent<DamageTrail>().damageUpdateInterval = 0.25f;
            };

            IL.RoR2.CharacterBody.UpdateFireTrail += CharacterBody_UpdateFireTrail;
            IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void CharacterBody_UpdateFireTrail(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchMul(),
                x => x.MatchStfld<DamageTrail>(nameof(DamageTrail.damagePerSecond))
                );

            if (ilfound)
            {
                c.Next.Operand = 1.2f;
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageInfo = damageReport.damageInfo;
            if (damageInfo == null)
            {
                return;
            }
            if (damageReport.dotType != DotController.DotIndex.None)
            {
                return;
            }
            if ((damageInfo.damageType & DamageType.IgniteOnHit) > DamageType.Generic || (damageReport.attackerBody && damageReport.attackerBody.HasBuff(RoR2Content.Buffs.AffixRed)))
            {
                uint? maxStacksFromAttacker = damageInfo.inflictor && damageInfo.inflictor.TryGetComponent(out ProjectileDamage projectileDamage) && projectileDamage.useDotMaxStacksFromAttacker ? 
                    projectileDamage.dotMaxStacksFromAttacker : null;
                InflictDotInfo inflictDotInfo = new InflictDotInfo
                {
                    attackerObject = damageReport.attacker,
                    victimObject = damageReport.victim.gameObject,
                    totalDamage = new float?(damageReport.damageInfo.damage * 0.5f),
                    damageMultiplier = 1f,
                    dotIndex = DotController.DotIndex.Burn,
                    maxStacksFromAttacker = maxStacksFromAttacker
                };
                if (damageReport.attackerMaster?.inventory)
                {
                    StrengthenBurnUtils.CheckDotForUpgrade(damageReport.attackerMaster.inventory, ref inflictDotInfo);
                }
                DotController.InflictDot(ref inflictDotInfo);
            }
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel breakLabel = null;
            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(out _),
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs).GetField(nameof(RoR2Content.Buffs.AffixRed))),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchBrfalse(out breakLabel)
                ) && c.TryGotoPrev(MoveType.Before,
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damageType)),
                x => x.MatchLdcI4((int)DamageType.IgniteOnHit),
                x => x.MatchAnd(),
                x => x.MatchLdcI4(0),
                x => x.MatchCgtUn(),
                x => x.MatchBrtrue(out _)
                );

            if (ilfound)
            {
                c.MoveAfterLabels();
                c.Emit(OpCodes.Br, breakLabel);
                c.EmitDelegate<Action>(() => LogWarning("Break!"));
                /*int startIndex = c.Index;
                ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt(typeof(StrengthenBurnUtils).GetMethod(nameof(StrengthenBurnUtils.CheckDotForUpgrade))),
                x => x.MatchLdloca(out _),
                x => x.MatchCallOrCallvirt<DotController>(nameof(DotController.InflictDot))
                );

                if (ilfound)
                {
                    int endIndex = c.Index;
                    c.Index = startIndex;
                    c.RemoveRange(endIndex - startIndex);
                }*/
            }
        }
    }
}
