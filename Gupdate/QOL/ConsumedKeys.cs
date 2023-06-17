using System;
using BepInEx;
using EntityStates.FlyingVermin.Weapon;
using EntityStates.Vermin.Weapon;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gupdate.QOL
{
    public class ConsumedKeys : ModBehaviour
    {
        public static ItemDef KeyConsumed { get; private set; } = ScriptableObject.CreateInstance<ItemDef>();
        public static ItemDef KeyVoidConsumed { get; private set; } = ScriptableObject.CreateInstance<ItemDef>();

        public override (string, string)[] GetLang() => new[]
        {
            (KeyConsumed.nameToken, "Rusted Key (Broken)"),
            (KeyConsumed.pickupToken, "It has fulfilled its purpose."),
            (KeyConsumed.descriptionToken, "A spent item with no remaining power."),
            (KeyVoidConsumed.nameToken, "Encrusted Key (Broken)"),
            (KeyVoidConsumed.pickupToken, "It has fulfilled its purpose."),
            (KeyVoidConsumed.descriptionToken, "A spent item with no remaining power."),
        };

        public void Awake()
        {
            On.RoR2.CostTypeCatalog.Init += CostTypeCatalog_Init;

            KeyConsumed.name = "KeyConsumed";
            KeyConsumed.nameToken = "GS_ITEM_KEYCONSUMED_NAME";
            KeyConsumed.pickupToken = "GS_ITEM_KEYCONSUMED_PICKUP";
            KeyConsumed.descriptionToken = "GS_ITEM_KEYCONSUMED_DESC";
            KeyConsumed.loreToken = "GS_ITEM_KEYCONSUMED_LORE";
            KeyConsumed.pickupIconSprite = Gupdate.assets.LoadAsset<Sprite>("texKeyConsumedIcon");
#pragma warning disable CS0618
            KeyConsumed.deprecatedTier = ItemTier.NoTier;
#pragma warning restore CS0618
            KeyConsumed.canRemove = false;
            KeyConsumed.tags = new[] { ItemTag.Utility };
            ContentAddition.AddItemDef(KeyConsumed);

            KeyVoidConsumed.name = "KeyVoidConsumed";
            KeyVoidConsumed.nameToken = "GS_ITEM_KEYVOIDCONSUMED_NAME";
            KeyVoidConsumed.pickupToken = "GS_ITEM_KEYVOIDCONSUMED_PICKUP";
            KeyVoidConsumed.descriptionToken = "GS_ITEM_KEYVOIDCONSUMED_DESC";
            KeyVoidConsumed.loreToken = "GS_ITEM_KEYVOIDCONSUMED_LORE";
            KeyVoidConsumed.pickupIconSprite = Gupdate.assets.LoadAsset<Sprite>("texKeyVoidConsumedIcon");
#pragma warning disable CS0618
            KeyVoidConsumed.deprecatedTier = ItemTier.NoTier;
#pragma warning restore CS0618
            KeyVoidConsumed.canRemove = false;
            KeyVoidConsumed.tags = new[] { ItemTag.Utility };
            KeyVoidConsumed.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();
            ContentAddition.AddItemDef(KeyVoidConsumed);
        }

        private void CostTypeCatalog_Init(On.RoR2.CostTypeCatalog.orig_Init orig)
        {
            orig();

            CostTypeDef treasureCacheItem = CostTypeCatalog.GetCostTypeDef(CostTypeIndex.TreasureCacheItem);
            CostTypeDef.PayCostDelegate giveConsumedKey = (CostTypeDef costTypeDef, CostTypeDef.PayCostContext context) =>
            {
                context.activatorBody.inventory.GiveItem(KeyConsumed, context.cost);
                CharacterMasterNotificationQueue.SendTransformNotification(context.activatorMaster, RoR2Content.Items.TreasureCache.itemIndex, KeyConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            };
            treasureCacheItem.payCost = (CostTypeDef.PayCostDelegate)Delegate.Combine(treasureCacheItem.payCost, giveConsumedKey);

            CostTypeDef treasureCacheVoidItem = CostTypeCatalog.GetCostTypeDef(CostTypeIndex.TreasureCacheVoidItem);
            CostTypeDef.PayCostDelegate giveConsumedKeyVoid = (CostTypeDef costTypeDef, CostTypeDef.PayCostContext context) =>
            {
                context.activatorBody.inventory.GiveItem(KeyVoidConsumed, context.cost);
                CharacterMasterNotificationQueue.SendTransformNotification(context.activatorMaster, DLC1Content.Items.TreasureCacheVoid.itemIndex, KeyVoidConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            };
            treasureCacheVoidItem.payCost = (CostTypeDef.PayCostDelegate)Delegate.Combine(treasureCacheVoidItem.payCost, giveConsumedKeyVoid);
        }
    }
}
