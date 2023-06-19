using System;
using BepInEx;
using EntityStates.Croco;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Skills;

namespace Gupdate.Gameplay.Monsters
{
    public class Acrid : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("CROCO_PASSIVE_ALT_DESCRIPTION", "Attacks that apply <style=cIsHealing>Poison</style> apply stacking <style=cIsDamage>Blight</style> instead, dealing <style=cIsDamage>60% damage per second</style> and reducing armor by <style=cIsDamage>5</style>."),
            ("CROCO_UTILITY_ALT1_DESCRIPTION", "<style=cIsDamage>Stunning</style>. Leap in the air, dealing <style=cIsDamage>600% damage</style>. <style=cIsUtility>Reduce</style> the cooldown by <style=cIsUtility>2s</style> for every enemy hit."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Croco/EntityStates.Croco.ChainableLeap.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(ChainableLeap.blastDamageCoefficient), 6f);
            };

            On.RoR2.Skills.SteppedSkillDef.OnFixedUpdate += SteppedSkillDef_OnFixedUpdate;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void SteppedSkillDef_OnFixedUpdate(On.RoR2.Skills.SteppedSkillDef.orig_OnFixedUpdate orig, SteppedSkillDef self, GenericSkill skillSlot)
        {
            if (self.skillName == "CrocoSlash" && self.canceledFromSprinting && skillSlot.characterBody.isSprinting && skillSlot.stateMachine.state.GetType() == self.activationState.stateType)
            {
                ((SteppedSkillDef.InstanceData)skillSlot.skillInstanceData).step = 0;
            }
            orig(self, skillSlot);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.armorAdd -= sender.GetBuffCount(RoR2Content.Buffs.Blight) * 5f;
        }
    }
}
