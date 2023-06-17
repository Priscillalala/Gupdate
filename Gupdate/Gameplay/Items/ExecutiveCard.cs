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
    public class ExecutiveCard : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("EQUIPMENT_MULTISHOPCARD_PICKUP", "Multishops remain open."),
            ("EQUIPMENT_MULTISHOPCARD_DESC", "Whenever you purchase a <style=cIsUtility>multishop</style> terminal, the other terminals will <style=cIsUtility>remain open</style>."),
        };

        public void Awake()
        {
            On.RoR2.Items.MultiShopCardUtils.OnPurchase += MultiShopCardUtils_OnPurchase;
        }

        private void MultiShopCardUtils_OnPurchase(On.RoR2.Items.MultiShopCardUtils.orig_OnPurchase orig, CostTypeDef.PayCostContext context, int moneyCost)
        {
            orig(context, 0);
        }
    }
}
