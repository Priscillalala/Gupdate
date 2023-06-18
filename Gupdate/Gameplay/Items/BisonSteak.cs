﻿using System;
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
    public class BisonSteak : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_FLATHEALTH_PICKUP", "Gain 25 max health and slightly increase regeneration."),
            ("ITEM_FLATHEALTH_DESC", "Increases <style=cIsHealing>maximum health</style> by <style=cIsHealing>25</style> <style=cStack>(+25 per stack)</style> and <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+0.2 hp/s<style=cStack>(+0.2 hp/s per stack)</style>."),
        };

        public void Awake()
        {
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(JunkContent.Buffs).GetField(nameof(JunkContent.Buffs.MeatRegenBoost))),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchBrtrue(out _),
                x => x.MatchLdcR4(out _),
                x => x.MatchBr(out _),
                x => x.MatchLdcR4(out _)
                );

            if (ilfound)
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<CharacterBody, float>>(body => 0.2f * body.inventory.GetItemCount(RoR2Content.Items.FlatHealth));
                c.Emit(OpCodes.Add);
            }
        }
    }
}
