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
using HG;

namespace Gupdate.Gameplay.Items
{
    public class SaferSpaces : ModBehaviour
    {
        public static BuffDef SaferSpace { get; private set; } = ScriptableObject.CreateInstance<BuffDef>();

        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_BEARVOID_PICKUP", "Gain brief invulnerability after being attacked. <style=cIsVoid>Corrupts all Tougher Times</style>."),
            ("ITEM_BEARVOID_DESC", "Gain <style=cIsHealing>0.2</style> <style=cStack>(+0.4 per stack)</style> seconds of invulnerability after being attacked. <style=cIsVoid>Corrupts all Tougher Times</style>."),
        };

        public void Awake()
        {
            ItemBehaviourUnlinker.Add<BearVoidBehavior>();

            SaferSpace.buffColor = new Color32(174, 108, 209, 255);
            SaferSpace.canStack = false;
            SaferSpace.isDebuff = false;
            SaferSpace.isCooldown = false;
            SaferSpace.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/BearVoid/texBuffBearVoidReady.tif").WaitForCompletion();
			ContentAddition.AddBuffDef(SaferSpace);

            TempVisualEffectAPI.AddTemporaryVisualEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/BearVoidEffect"), body => body.HasBuff(SaferSpace));
        }

		public class BearVoidBehaviour : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver, IOnTakeDamageServerReceiver
		{
			[ItemDefAssociation(useOnServer = true, useOnClient = false)]
			public static ItemDef GetItemDef() => DLC1Content.Items.BearVoid;

			public void OnEnable()
			{
				if (body.healthComponent)
				{
					ArrayUtils.ArrayAppend(ref body.healthComponent.onIncomingDamageReceivers, this);
					ArrayUtils.ArrayAppend(ref body.healthComponent.onTakeDamageReceivers, this);
				}
			}

			public void OnDisable()
			{
				if (body.healthComponent)
                {
					int index;
					if ((index = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this)) >= 0) 
					{
						ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, index);
					}
					if ((index = Array.IndexOf(body.healthComponent.onTakeDamageReceivers, this)) >= 0)
					{
						ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onTakeDamageReceivers, index);
					}
				}
			}

			public void OnIncomingDamageServer(DamageInfo damageInfo)
			{
				bool bypassBlock = (damageInfo.damageType & DamageType.BypassBlock) > DamageType.Generic;
				if (!bypassBlock && damageInfo.damage > 0f)				{
					EffectData effectData = new EffectData
					{
						origin = damageInfo.position,
						rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
					};
					EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearVoidEffectPrefab, effectData, true);
					
					damageInfo.rejected = true;
				}
			}

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (damageReport.attacker && damageReport.dotType == DotController.DotIndex.None)
                {
					body.AddTimedBuff(SaferSpace, Gutil.StackScaling(0.2f, 0.4f, stack));
                }
            }
        }
	}
}
