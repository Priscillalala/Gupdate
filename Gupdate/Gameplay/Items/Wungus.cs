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

namespace Gupdate.Gameplay.Items
{
    public class Wungus : ModBehaviour
    {
        public static BuffDef MushroomRegen { get; private set; } = ScriptableObject.CreateInstance<BuffDef>();

        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_MUSHROOMVOID_PICKUP", "Gain a stacking regeneration boost on kill. <style=cIsVoid>Corrupts all Bustling Fungi</style>."),
            ("ITEM_MUSHROOMVOID_DESC", "On killing an enemy, boost <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+1.5 hp/s</style> for <style=cIsUtility>1s (+1s per stack)</style> and refresh all current boosts. <style=cIsVoid>Corrupts all Bustling Fungi</style>."),
        };

        public void Awake()
        {
            ItemBehaviourUnlinker.Add<MushroomVoidBehavior>();

            MushroomRegen.buffColor = new Color32(247, 173, 250, 255);
            MushroomRegen.canStack = true;
            MushroomRegen.isCooldown = false;
            MushroomRegen.isDebuff = false;
            MushroomRegen.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/MushroomVoid/texBuffMushroomVoidIcon.tif").WaitForCompletion();

            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MushroomVoid/MushroomVoidEffect.prefab").Completed += handle =>
            {
                TempVisualEffectAPI.AddTemporaryVisualEffect(handle.Result, x => x.HasBuff(MushroomRegen));
            };

            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            if (buffDef == MushroomRegen)
            {
                for (int i = 0; i < self.timedBuffs.Count; i++)
                {
                    CharacterBody.TimedBuff timedBuff = self.timedBuffs[i];
                    if (timedBuff.buffIndex == buffDef.buffIndex)
                    {
                        if (timedBuff.timer < duration)
                        {
                            timedBuff.timer = duration;
                        }
                    }
                }
            }
            orig(self, buffDef, duration);
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.attackerBody && damageReport.attackerBody.HasItem(DLC1Content.Items.MushroomVoid, out int stack))
            {
                damageReport.attackerBody.AddTimedBuff(MushroomRegen, stack);
            }
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
                c.EmitDelegate<Func<CharacterBody, float>>(body => 1.5f * body.GetBuffCount(MushroomRegen));
                c.Emit(OpCodes.Add);
            }
        }
    }
}
