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

namespace Gupdate.Gameplay.Items
{
    public class Opal : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_OUTOFCOMBATARMOR_DESC", "<style=cIsHealing>Increase armor</style> by <style=cIsHealing>60</style> <style=cStack>(+60 per stack)</style> while out of danger."),
        };

        public void Awake()
        {
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC1Content.Buffs).GetField(nameof(DLC1Content.Buffs.OutOfCombatArmorBuff))),
                x => x.MatchCall<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchBrtrue(out _),
                x => x.MatchLdcR4(out _),
                x => x.MatchBr(out _),
                x => x.MatchLdcR4(out _)
                );
            if (ilfound)
            {
                c.Prev.Operand = 60f;
            }
        }
    }
}
