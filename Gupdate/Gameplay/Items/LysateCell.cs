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
using Mono.Cecil.Cil;

namespace Gupdate.Gameplay.Items
{
    public class LysateCell : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_EQUIPMENTMAGAZINEVOID_PICKUP", "Add an extra charge of your Special skill. Reduce Special skill cooldown. <style=cIsVoid>Corrupts all Fuel Cells</style>."),
            ("ITEM_EQUIPMENTMAGAZINEVOID_DESC", "Add <style=cIsUtility>+1</style> <style=cStack>(+1 per stack)</style> charge of your <style=cIsUtility>Special skill</style>. <style=cIsUtility>Reduces Special skill cooldown</style> by <style=cIsUtility>15%</style> <style=cStack>(+15% per stack)</style>. <style=cIsVoid>Corrupts all Fuel Cells.</style>."),
        };

        public void Awake()
        {
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(DLC1Content), nameof(DLC1Content.Items.EquipmentMagazineVoid), out int locStackIndex))
            {
                ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(locStackIndex),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out _)
                ) && c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<GenericSkill>("get_cooldownScale"),
                x => x.MatchLdcR4(out _),
                x => x.MatchMul(),
                x => x.MatchCallOrCallvirt<GenericSkill>("set_cooldownScale")
                );

                if (ilfound)
                {
                    c.Index += 2;
                    c.Emit(OpCodes.Ldloc, locStackIndex);
                    c.EmitDelegate<Func<float, int, float>>((reduction, stack) => 1 / (1 + (0.15f * stack)));
                }
            }
        }
    }
}
