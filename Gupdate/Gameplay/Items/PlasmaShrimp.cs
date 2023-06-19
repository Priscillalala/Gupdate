using System;
using BepInEx;
using EntityStates.FlyingVermin.Weapon;
using EntityStates.Vermin.Weapon;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Items;
using Mono.Cecil.Cil;

namespace Gupdate.Gameplay.Items
{
    public class PlasmaShrimp : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_MISSILEVOID_DESC", "Gain a <style=cIsHealing>shield</style> equal to <style=cIsHealing>4%</style> <style=cStack>(+4% per stack)</style> of your maximum health. While you have a <style=cIsHealing>shield</style>, hitting an enemy fires a missile that deals <style=cIsDamage>30%</style> <style=cStack>(+60% per stack)</style> base damage. <style=cIsVoid>Corrupts all AtG Missile Mk. 1s</style>."),
        };

        public void Awake()
        {
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(DLC1Content), nameof(DLC1Content.Items.MissileVoid), out int locStackIndex))
            {
                ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(locStackIndex),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out _)
                ) && c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<CharacterBody>("get_maxHealth"),
                x => x.MatchLdcR4(out _),
                x => x.MatchMul()
                );

                if (ilfound)
                {
                    c.Previous.Previous.Operand = 0.04f;
                    c.Emit(OpCodes.Ldloc, locStackIndex);
                    c.Emit(OpCodes.Conv_R4);
                    c.Emit(OpCodes.Mul);
                }
            }
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(DLC1Content), nameof(DLC1Content.Items.MissileVoid), out int locStackIndex))
            {
                ILLabel breakLabel = null;
                ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(locStackIndex),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out breakLabel)
                );

                if (ilfound)
                {
                    c.Emit(OpCodes.Ldarg, 1);
                    c.EmitDelegate<Func<DamageInfo, bool>>(damageInfo => Util.CheckRoll(100f * damageInfo.procCoefficient, damageInfo.attacker?.GetComponent<CharacterBody>()?.master));
                    c.Emit(OpCodes.Brfalse, breakLabel);
                }

                int locAttackerBodyIndex = -1;
                ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out locAttackerBodyIndex),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_damage"),
                x => x.MatchLdloc(out _),
                x => x.MatchCallOrCallvirt(typeof(Util).GetMethod(nameof(Util.OnHitProcDamage))),
                x => x.MatchLdloc(out _),
                x => x.MatchMul()
                );

                if (ilfound)
                {
                    c.Emit(OpCodes.Ldloc, locAttackerBodyIndex);
                    c.EmitDelegate<Func<float, CharacterBody, float>>((_, attackerBody) => 
                    {
                        return attackerBody.damage * Gutil.StackScaling(0.3f, 0.6f, attackerBody.inventory.GetItemCount(DLC1Content.Items.MissileVoid));
                    });
                }
            }
        }
    }
}
