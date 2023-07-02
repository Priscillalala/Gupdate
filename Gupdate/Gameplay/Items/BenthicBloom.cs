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
using System.Collections.Generic;
using Mono.Cecil;

namespace Gupdate.Gameplay.Items
{
    public class BenthicBloom : ModBehaviour
    {
        //private static List<PickupIndex> tier1DropList;
        //private static ItemDef currentItemDef;

        public void Awake()
        {
            //IL.RoR2.CharacterMaster.OnServerStageBegin += CharacterMaster_OnServerStageBegin;
            On.RoR2.CharacterMaster.OnServerStageBegin += CharacterMaster_OnServerStageBegin;
            //IL.RoR2.CharacterMaster.TryCloverVoidUpgrades += CharacterMaster_TryCloverVoidUpgrades;
        }

        private void CharacterMaster_OnServerStageBegin(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            if (NetworkServer.active)
            {
                self.TryRegenerateScrap();
            }
            orig(self, stage);
        }

        /*private void CharacterMaster_OnServerStageBegin(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            bool found = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterMaster>(nameof(CharacterMaster.TryCloverVoidUpgrades))
            );
            if (found)
            {
                GSUtil.Log("before");
                c.RemoveRange(2);
                GSUtil.Log("after");
                //c.Index = c.Instrs.Count - 4;
                //GSUtil.Log(c.Index);
                //c.Emit(OpCodes.Ldarg_0);
                //c.EmitDelegate<Action<CharacterMaster>>((master) => master.TryCloverVoidUpgrades());
            }
        }*/

        /*private void CharacterMaster_TryCloverVoidUpgrades(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchCall<Run>("get_instance"),
                x => x.MatchLdfld<Run>(nameof(Run.availableTier2DropList))
            );
            if (ilfound)
            {
                c.EmitDelegate<Action>(delegate
                {
                    tier1DropList = new List<PickupIndex>(Run.instance.availableTier1DropList);
                });
            }

            int locPickupListIndex = -1;
            FieldReference startingItemDef = null;
            ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdnull(),
                x => x.MatchStloc(out int i),
                x => x.MatchLdnull(),
                x => x.MatchStloc(out locPickupListIndex),
                x => x.MatchLdloc(out int i),
                x => x.MatchLdfld(out startingItemDef)
            );
            if (ilfound)
            {
                c.EmitDelegate<Func<ItemDef, ItemDef>>((ItemDef itemDef) =>
                {
                    currentItemDef = itemDef;
                    return itemDef;
                });
            }

            ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchCallvirt<ItemDef>("get_tier"),
                x => x.MatchStloc(out int i)
            );
            if (ilfound)
            {
                c.Emit(OpCodes.Ldloc, locPickupListIndex);
                c.EmitDelegate<Func<List<PickupIndex>, List<PickupIndex>>>((List<PickupIndex> pickupList) =>
                {
                    return currentItemDef && currentItemDef.tier == ItemTier.NoTier && !currentItemDef.hidden ? tier1DropList : pickupList;
                });
                c.Emit(OpCodes.Stloc, locPickupListIndex);
            }

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(out int i),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out ILLabel label)
            );
            if (ilfound)
            {
                c.MoveAfterLabels();
                c.EmitDelegate<Action>(delegate
                {
                    currentItemDef = null;
                    tier1DropList = null;
                });
            }

        }*/
    }
}
