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
using EntityStates.Merc;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates;

namespace Gupdate.Gameplay.Monsters
{
    public class Merc : ModBehaviour
    {
        public void Awake()
        {
            IL.EntityStates.Merc.EvisDash.FixedUpdate += EvisDash_FixedUpdate;
            IL.EntityStates.Merc.WhirlwindBase.FixedUpdate += WhirlwindBase_FixedUpdate;
            IL.EntityStates.Merc.Uppercut.FixedUpdate += Uppercut_FixedUpdate;
        }

        private void EvisDash_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel breakLabel = null;
            int locHurtBoxIndex = -1;
            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchNewobj<Evis>()
                ) && c.TryGotoPrev(MoveType.After,
                x => x.MatchLdloc(out locHurtBoxIndex),
                x => x.MatchLdfld<HurtBox>(nameof(HurtBox.healthComponent)),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EntityState>("get_healthComponent"),
                x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Inequality"),
                x => x.MatchBrfalse(out breakLabel)
                );

            if (ilfound)
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.Emit(OpCodes.Ldloc, locHurtBoxIndex);
                c.EmitDelegate<Func<EvisDash, HurtBox, bool>>((evisDash, hurtBox) => FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off || evisDash.teamComponent.teamIndex != hurtBox.teamIndex);
                c.Emit(OpCodes.Brfalse, breakLabel);
            }
        }

        private void WhirlwindBase_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = c.TryGotoNext(MoveType.After, x => x.MatchLdfld<WhirlwindBase>(nameof(WhirlwindBase.moveSpeedBonusCoefficient))))
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<WhirlwindBase, float>>(uppercut => uppercut.attackSpeedStat);
                c.Emit(OpCodes.Mul);
            }
        }

        private void Uppercut_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<Uppercut>(nameof(Uppercut.moveSpeedBonusCoefficient)))) 
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<Uppercut, float>>(uppercut => uppercut.attackSpeedStat);
                c.Emit(OpCodes.Mul);
            }
        }
    }
}
