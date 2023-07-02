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
using HG;
using Mono.Cecil.Cil;

namespace Gupdate.Gameplay.Items
{
    public class Knurl : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_KNURL_PICKUP", "Boosts armor and regeneration as health is lost."),
            ("ITEM_KNURL_DESC", "<style=cIsHealing>Increase armor</style> by up to <style=cIsHealing>20</style> <style=cStack>(+20 per stack)</style> and <style=cIsHealing>base health regeneration</style> by up to <style=cIsHealing>+3.6 hp/s</style> <style=cStack>(+3.6 hp/s per stack)</style> after taking damage. Gain more the <style=cIsHealth>lower your health is</style>."),
        };

        public void Awake()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
			sender.armor += KnurlBehaviour.GetArmorBoost(sender);
        }

		private void CharacterBody_RecalculateStats(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(RoR2Content), nameof(RoR2Content.Items.Knurl), out int _))
			{
				c.Index--;
				c.Emit(OpCodes.Pop);
				c.Emit(OpCodes.Ldc_I4_0);

				int locMultiplierIndex = -1;
				int locMeatRegenBoostIndex = -1;
				ilfound = c.TryGotoNext(MoveType.After,
					x => x.MatchLdsfld(typeof(JunkContent.Buffs).GetField(nameof(JunkContent.Buffs.MeatRegenBoost))),
					x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
					x => x.MatchBrtrue(out _),
					x => x.MatchLdcR4(out _),
					x => x.MatchBr(out _),
					x => x.MatchLdcR4(out _),
					x => x.MatchLdloc(out locMultiplierIndex),
					x => x.MatchMul(),
					x => x.MatchStloc(out locMeatRegenBoostIndex)
					) && c.TryGotoNext(MoveType.After,
					x => x.MatchLdloc(locMeatRegenBoostIndex),
					x => x.MatchAdd()
					);

				if (ilfound)
				{
					c.Emit(OpCodes.Ldarg, 0);
					c.Emit(OpCodes.Ldloc, locMultiplierIndex);
					c.EmitDelegate<Func<CharacterBody, float, float>>((body, mult) => mult * KnurlBehaviour.GetRegenBoost(body));
					c.Emit(OpCodes.Add);
				}
			}
		}

        public class KnurlBehaviour : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
		{
			[ItemDefAssociation(useOnServer = true, useOnClient = false)]
			public static ItemDef GetItemDef() => RoR2Content.Items.Knurl;

			private float currentBoostCoefficient;

			public void OnEnable()
			{
				if (body.healthComponent)
				{
					ArrayUtils.ArrayAppend(ref body.healthComponent.onTakeDamageReceivers, this);
				}
				RecalculateCoefficient();
			}

			public void OnDisable()
			{
				RecalculateCoefficient();
				if (body.healthComponent)
				{
					int index;
					if ((index = Array.IndexOf(body.healthComponent.onTakeDamageReceivers, this)) >= 0)
					{
						ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onTakeDamageReceivers, index);
					}
				}
			}

			public void OnTakeDamageServer(DamageReport _)
			{
				RecalculateCoefficient();
			}

			public void RecalculateCoefficient()
            {
				currentBoostCoefficient = Mathf.Clamp01(1f - body.healthComponent.combinedHealthFraction);
				body.MarkAllStatsDirty();
            }

			public static float GetRegenBoost(CharacterBody body)
            {
				if (body && body.HasItem(RoR2Content.Items.Knurl) && body.TryGetComponent(out KnurlBehaviour knurlBehaviour))
                {
					return knurlBehaviour.stack * knurlBehaviour.currentBoostCoefficient * 3.6f;
                }
				return 0f;
            }

			public static float GetArmorBoost(CharacterBody body)
			{
				if (body && body.HasItem(RoR2Content.Items.Knurl) && body.TryGetComponent(out KnurlBehaviour knurlBehaviour))
				{
					return knurlBehaviour.stack * knurlBehaviour.currentBoostCoefficient * 20f;
				}
				return 0f;
			}
		}
	}
}
