using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using EntityStates.VoidSurvivor.Weapon;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates;
using UnityEngine.UI;
using RoR2.Skills;
using System.Linq;
using HG;
using RoR2.UI;

namespace Gupdate.Gameplay.Monsters
{
    public class VoidFiend : ModBehaviour
    {
        private AsyncOperationHandle<SkillDef> crushHealth;

        public override (string, string)[] GetLang() => new[]
        {
            ("GS_VOIDSURVIVOR_PRIMARY_CORRUPT_DESCRIPTION", "Rapidly fire a short-range beam for <style=cIsDamage>2000% damage per second</style>."),
            ("GS_VOIDSURVIVOR_SECONDARY_CORRUPT_DESCRIPTION", "Instantly fire an arcing bomb, exploding for <style=cIsDamage>1100% damage</style> in a large radius."),
            ("GS_VOIDSURVIVOR_UTILITY_CORRUPT_DESCRIPTION", "<style=cIsUtility>Disappear</style> into the Void, <style=cIsUtility>cleansing all debuffs</style> while aggressively <style=cIsDamage>dashing forwards</style>."),
            ("GS_VOIDSURVIVOR_SPECIAL_CORRUPT_DESCRIPTION", "Crush <style=cIsHealth>25% maximum health</style> to gain <style=cIsVoid>25% Corruption</style>."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/FireCorruptBeam.asset").Completed += handle => handle.Result.skillDescriptionToken = "GS_VOIDSURVIVOR_PRIMARY_CORRUPT_DESCRIPTION";
            Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/FireCorruptDisk.asset").Completed += handle => handle.Result.skillDescriptionToken = "GS_VOIDSURVIVOR_SECONDARY_CORRUPT_DESCRIPTION";
            Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/VoidBlinkDown.asset").Completed += handle => handle.Result.skillDescriptionToken = "GS_VOIDSURVIVOR_UTILITY_CORRUPT_DESCRIPTION";

            crushHealth = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/CrushHealth.asset");
            crushHealth.Completed += handle => handle.Result.skillDescriptionToken = "GS_VOIDSURVIVOR_SPECIAL_CORRUPT_DESCRIPTION";

            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab").Completed += handle =>
            {
                if (handle.Result.TryGetComponent(out VoidSurvivorController voidSurvivorController))
                {
                    voidSurvivorController.corruptionForFullHeal = -50f;
                }
            };

            foreach (string key in new[]
            {
                "RoR2/DLC1/VoidSurvivor/EntityStates.VoidSurvivor.Weapon.ChargeCrushCorruption.asset",
                "RoR2/DLC1/VoidSurvivor/EntityStates.VoidSurvivor.Weapon.ChargeCrushHealth.asset",
            })
            {
                Addressables.LoadAssetAsync<EntityStateConfiguration>(key).Completed += handle =>
                {
                    handle.Result.TryModifyFieldValue(nameof(ChargeCrushBase.baseDuration), 0.8f);
                };
            }
            foreach (string key in new[]
            {
                "RoR2/DLC1/VoidSurvivor/EntityStates.VoidSurvivor.Weapon.CrushCorruption.asset",
                "RoR2/DLC1/VoidSurvivor/EntityStates.VoidSurvivor.Weapon.CrushHealth.asset",
            })
            {
                Addressables.LoadAssetAsync<EntityStateConfiguration>(key).Completed += handle =>
                {
                    handle.Result.TryModifyFieldValue(nameof(CrushBase.baseDuration), 0.8f);
                };
            }

            Addressables.LoadAssetAsync<VoidSurvivorSkillDef>("RoR2/DLC1/VoidSurvivor/CrushCorruption.asset").Completed += handle =>
            {
                handle.Result.baseMaxStock = 2;
                handle.Result.rechargeStock = 0;
            };

            foreach (string key in new[]
            {
                "RoR2/DLC1/VoidSurvivor/matVoidSurvivorFlesh.mat",
                "RoR2/DLC1/VoidSurvivor/matVoidSurvivorHead.mat",
                "RoR2/DLC1/VoidSurvivor/matVoidSurvivorMetal.mat",
            })
            {
                Addressables.LoadAssetAsync<Material>(key).Completed += handle =>
                {
                    handle.Result.EnableKeyword("DITHER");
                };
            }

            On.RoR2.VoidSurvivorController.UpdateUI += VoidSurvivorController_UpdateUI;
            IL.RoR2.UI.SkillIcon.Update += SkillIcon_Update;
            On.RoR2.Skills.SkillDef.IsReady += SkillDef_IsReady;
            //IL.RoR2.Skills.VoidSurvivorSkillDef.HasRequiredCorruption += VoidSurvivorSkillDef_HasRequiredCorruption;
            //On.RoR2.VoidSurvivorController.UpdateUI += VoidSurvivorController_UpdateUI;
        }

        private void VoidSurvivorController_UpdateUI(On.RoR2.VoidSurvivorController.orig_UpdateUI orig, VoidSurvivorController self)
        {
            if (self.corruption > self.maxCorruption)
            {
                self._corruption = self.maxCorruption;
            }
            orig(self);
        }

        private void SkillIcon_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.bodyColor))
                );

            if (ilfound)
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<Color, SkillIcon, Color>>((color, skillIcon) => 
                {
                    if (skillIcon.targetSkill.characterBody.HasBuff(DLC1Content.Buffs.VoidSurvivorCorruptMode))
                    {
                        return new Color32(237, 18, 57, 255);
                    }
                    return color;
                });
            }
        }

        private bool SkillDef_IsReady(On.RoR2.Skills.SkillDef.orig_IsReady orig, SkillDef self, GenericSkill skillSlot)
        {
            if (self.skillName == "CrocoLeap" && self == crushHealth.WaitForCompletion() && !CanCrushHealth(skillSlot))
            {
                return false;
            }
            return orig(self, skillSlot);
        }

        public bool CanCrushHealth(GenericSkill skillSlot)
        {
            return skillSlot?.characterBody?.healthComponent && skillSlot.characterBody.healthComponent.combinedHealthFraction > 0.25f;
        }

        /*private void VoidSurvivorSkillDef_HasRequiredCorruption(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int locInstanceIndex = -1;
            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(out locInstanceIndex),
                x => x.MatchLdfld<VoidSurvivorSkillDef.InstanceData>(nameof(VoidSurvivorSkillDef.InstanceData.voidSurvivorController)),
                x => x.MatchCallOrCallvirt<VoidSurvivorController>("get_corruption"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<VoidSurvivorSkillDef>(nameof(VoidSurvivorSkillDef.minimumCorruption))
                );

            if (ilfound)
            {
                c.Index += 3;
                c.Emit(OpCodes.Ldloc, locInstanceIndex);
                c.EmitDelegate<Func<float, VoidSurvivorSkillDef.InstanceData, float>>((corruption, instance) => corruption - instance.voidSurvivorController.minimumCorruption);
            }
        }

        private void VoidSurvivorController_UpdateUI(On.RoR2.VoidSurvivorController.orig_UpdateUI orig, VoidSurvivorController self)
        {
            orig(self);
            if (self.overlayInstanceChildLocator && self.overlayInstanceChildLocator.TryGetComponent(out VoidFillController voidFillController))
            {
                voidFillController.OnUpdateUI(self);
            }
        }

        public class VoidFillController : MonoBehaviour
        {
            private Image voidFill;

            public void Awake()
            {
                Transform fill = base.transform.Find("FillRoot/Fill/Fill");
                if (!fill)
                {
                    return;
                }
                voidFill = Instantiate(fill.gameObject, fill.parent).GetComponent<Image>();
                if (voidFill)
                {
                    voidFill.transform.SetAsLastSibling();
                    voidFill.GetComponent<Image>().color = new Color32(179, 64, 197, 255);
                }
            }

            public void OnUpdateUI(VoidSurvivorController voidSurvivorController)
            {
                Transform voidFillTransform = voidSurvivorController.overlayInstanceChildLocator.FindChild("GS_VoidFill");
                if (voidFill)
                {
                    if (voidSurvivorController.isCorrupted)
                    {
                        voidFill.enabled = false;
                    }
                    else
                    {
                        voidFill.enabled = true;
                        voidFill.fillAmount = voidSurvivorController.minimumCorruption / voidSurvivorController.maxCorruption;
                    }
                }
            }
        }*/

        /*Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorCorruptionUISimplified.prefab").Completed += handle =>
            {
                handle.Result.AddComponent<VoidFillController>();
                if (handle.Result.transform.TryFind("FillRoot/Fill", out Transform fillHolder))
                {
                    if (fillHolder.TryFind("MinCorruptionThreshold", out Transform minCorruptionThreshold))
                    {
                        minCorruptionThreshold.gameObject.SetActive(false);
                    }
                }
            };*/
    }
}
