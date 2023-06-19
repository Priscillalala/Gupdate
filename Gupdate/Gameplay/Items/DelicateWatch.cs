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
    public class DelicateWatch : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_FRAGILEDAMAGEBONUS_DESC", "Increase damage by <style=cIsDamage>12%</style> <style=cStack>(+12% per stack)</style>. Taking damage to below <style=cIsHealth>25% health</style> <style=cIsUtility>breaks</style> this item. <style=cIsUtility>Resets on the minute</style>."),
            ("ITEM_FRAGILEDAMAGEBONUSCONSUMED_PICKUP", "...well, it's still right twice a day. Resets on the minute."),
        };

        public void Awake()
        {
            IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locStackIndex = -1;
            ilfound = c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld(typeof(DLC1Content.Items).GetField(nameof(DLC1Content.Items.FragileDamageBonus))),
            x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
            x => x.MatchStloc(out locStackIndex)
            ) && c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(locStackIndex),
            x => x.MatchConvR4(),
            x => x.MatchLdcR4(out _),
            x => x.MatchMul()
            );
            if (ilfound)
            {
                c.Previous.Previous.Operand = 0.12f;
            }
        }

        public class FragileDamageBonusConsumedBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => DLC1Content.Items.FragileDamageBonusConsumed;


            private int previousMinute;

            public void Start()
            {
                previousMinute = GetCurrentMinute();
            }

            public void FixedUpdate()
            {
                int currentMinute = GetCurrentMinute();
                if (previousMinute < currentMinute && body.inventory.GetItemCount(DLC1Content.Items.FragileDamageBonusConsumed) > 0)
                {
                    previousMinute = currentMinute;
                    CharacterMasterNotificationQueue.SendTransformNotification(body.master, DLC1Content.Items.FragileDamageBonusConsumed.itemIndex, DLC1Content.Items.FragileDamageBonus.itemIndex, CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen);
                    body.inventory.GiveItem(DLC1Content.Items.FragileDamageBonus, 1);
                    body.inventory.RemoveItem(DLC1Content.Items.FragileDamageBonusConsumed, 1);
                }
            }

            private int GetCurrentMinute()
            {
                return Mathf.FloorToInt(Run.instance.GetRunStopwatch() / 60f);
            }
        }
    }
}
