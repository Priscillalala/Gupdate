using System;
using BepInEx;
using EntityStates.Railgunner.Backpack;
using EntityStates.Railgunner.Reload;
using EntityStates.Railgunner.Scope;
using EntityStates.Railgunner.Weapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Skills;
using EntityStates;
using JetBrains.Annotations;
using System.Linq;
using HG;

namespace Gupdate.Gameplay.Monsters
{
    public class Railgunner : ModBehaviour
    {
        private AsyncOperationHandle<RailgunSkillDef> scopeLight;
        private AsyncOperationHandle<GameObject> mineAltGhost;
        private AsyncOperationHandle<GameObject> mineAltGhostReskin;
        private AsyncOperationHandle<GameObject> mineAltProjectile;
        private static GameObject mineDestructionExplosion;

        public override (string, string)[] GetLang() => new[]
        {
            ("RAILGUNNER_SPECIAL_ALT_DESCRIPTION", "<style=cIsUtility>Freezing</style>. Fire <style=cIsDamage>piercing</style> round for <style=cIsDamage>2000% damage</style>."),
            ("GS_KEYWORD_PASSIVERELOAD", "<style=cKeywordName>Passive Reload</style><style=cSub><style=cIsUtility>Continuously load rounds</style> into your railgun while not firing. Hold up to 10."),
            ("RAILGUNNER_SECONDARY_ALT_DESCRIPTION", "Activate your <style=cIsUtility>short-range scope</style>, highlighting <style=cIsDamage>Weak Points</style> and transforming your weapon into a quick <style=cIsDamage>500% damage</style> railgun."),
            ("RAILGUNNER_SNIPE_LIGHT_DESCRIPTION", "Launch a light projectile for <style=cIsDamage>500% damage</style>.")
        };

        public void Awake()
        {
            ContentAddition.AddEntityState<EnterReloadLightSnipe>(out _);
            ContentAddition.AddEntityState<ReloadLightSnipe>(out _);
            ContentAddition.AddEntityState<ExitReloadLightSnipe>(out _);

            mineDestructionExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineExplosionAlt Variant.prefab").WaitForCompletion()
                .InstantiateClone("MineAltDestructionExplosion", false);
            mineDestructionExplosion.GetComponent<EffectComponent>().soundName = "Play_loader_R_expire";
            ContentAddition.AddEffect(mineDestructionExplosion);

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/DLC1/Railgunner/EntityStates.Railgunner.Weapon.FireSnipeLight.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(FireSnipeLight.useSecondaryStocks), true);
                handle.Result.TryModifyFieldValue(nameof(FireSnipeLight.damageCoefficient), 5f);
            };
            Addressables.LoadAssetAsync<RailgunSkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyFireSnipeLight.asset").Completed += handle =>
            {
                handle.Result.baseMaxStock = int.MaxValue;
                handle.Result.rechargeStock = 0;
            };
            scopeLight = Addressables.LoadAssetAsync<RailgunSkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyScopeLight.asset");
            scopeLight.Completed += handle =>
            {
                handle.Result.baseMaxStock = 10;
                handle.Result.restockOnReload = false;
                ArrayUtils.ArrayAppend(ref handle.Result.keywordTokens, "GS_KEYWORD_PASSIVERELOAD");
            };

            mineAltGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerAltMineGhost.prefab");
            mineAltGhostReskin = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerAltMineGhostReskin.prefab");
            mineAltProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAltDetonated.prefab");
            mineAltProjectile.Completed += handle =>
            {
                handle.Result.AddComponent<DetachMineAltFX>();
                handle.Result.GetComponent<ProjectileController>().ghostPrefab = mineAltGhost.WaitForCompletion();
                /*if (handle.Result.transform.TryFind("AreaIndicator/ChargeIn", out Transform chargeIn))
                {
                    handle.Result.AddComponent<DetachParticleOnDestroyAndEndEmission>().particleSystem = chargeIn.GetComponent<ParticleSystem>();
                }
                if (handle.Result.transform.TryFind("AreaIndicator/SoftGlow", out Transform softGlow))
                {
                    handle.Result.AddComponent<DetachParticleOnDestroyAndEndEmission>().particleSystem = softGlow.GetComponent<ParticleSystem>();
                }
                if (handle.Result.transform.TryFind("AreaIndicator/Core", out Transform core))
                {
                    handle.Result.AddComponent<DetachParticleOnDestroyAndEndEmission>().particleSystem = core.GetComponent<ParticleSystem>();
                }*/
            };
            Addressables.LoadAssetAsync<SkinDef>("RoR2/DLC1/Railgunner/skinRailGunnerAlt.asset").Completed += handle =>
            {
                ArrayUtils.ArrayAppend(ref handle.Result.projectileGhostReplacements, new SkinDef.ProjectileGhostReplacement
                {
                    projectilePrefab = mineAltProjectile.WaitForCompletion(),
                    projectileGhostReplacementPrefab = mineAltGhostReskin.WaitForCompletion(),
                });
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAlt.prefab").Completed += handle =>
            {
                handle.Result.GetComponent<ProjectileExplosion>().useLocalSpaceForChildren = true;
            };

            On.RoR2.Skills.RailgunSkillDef.OnAssigned += RailgunSkillDef_OnAssigned;
            On.RoR2.Skills.RailgunSkillDef.OnFixedUpdate += RailgunSkillDef_OnFixedUpdate;
        }

        private SkillDef.BaseSkillInstanceData RailgunSkillDef_OnAssigned(On.RoR2.Skills.RailgunSkillDef.orig_OnAssigned orig, RailgunSkillDef self, GenericSkill skillSlot)
        {
            if (self.skillName == "ScopeLight" && self == scopeLight.WaitForCompletion())
            {
                return new ReloadInstanceData
                {
                    backpackStateMachine = EntityStateMachine.FindByCustomName(skillSlot.gameObject, "Backpack"),
                    reloadStateMachine = EntityStateMachine.FindByCustomName(skillSlot.gameObject, "Reload"),
                    weaponStateMachine = EntityStateMachine.FindByCustomName(skillSlot.gameObject, "Weapon"),
                };
            }
            return orig(self, skillSlot);
        }

        private void RailgunSkillDef_OnFixedUpdate(On.RoR2.Skills.RailgunSkillDef.orig_OnFixedUpdate orig, RailgunSkillDef self, GenericSkill skillSlot)
        {
            orig(self, skillSlot);
            if (self.skillName == "ScopeLight" && self == scopeLight.WaitForCompletion() && skillSlot.stock < skillSlot.maxStock)
            {
                if (skillSlot.skillInstanceData is ReloadInstanceData instanceData && CanReload(instanceData))
                {
                    instanceData.weaponStateMachine.SetNextState(new EnterReloadLightSnipe());
                }
            }
        }

        public static bool CanReload(ReloadInstanceData instanceData)
        {
            if (!instanceData.weaponStateMachine)
            {
                return false;
            }
            if (instanceData.backpackStateMachine) 
            {
                if (instanceData.backpackStateMachine.HasPendingState()) 
                {
                    return false;
                }
                if (instanceData.backpackStateMachine.state is not (Disconnected or BaseOnline))
                {
                    return false;
                }
            }
            return !instanceData.weaponStateMachine.HasPendingState() && instanceData.weaponStateMachine.state is Idle;
        }

        public class ReloadInstanceData : RailgunSkillDef.InstanceData
        {
            public EntityStateMachine weaponStateMachine;
        }

        public class DetachMineAltFX : MonoBehaviour
        {
            public void OnDestroy()
            {
                Transform areaIndicator = base.transform.Find("AreaIndicator");
                foreach (ParticleSystem particleSystem in areaIndicator.GetComponentsInChildren<ParticleSystem>())
                {
                    var main = particleSystem.main;
                    main.stopAction = ParticleSystemStopAction.Destroy;
                    main.loop = false;
                }
                areaIndicator.gameObject.AddComponent<DestroyOnTimer>().duration = 0.2f;

                ObjectScaleCurve objectScaleCurve = areaIndicator.transform.Find("Sphere").gameObject.AddComponent<ObjectScaleCurve>();
                objectScaleCurve.overallCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
                objectScaleCurve.useOverallCurveOnly = true;
                objectScaleCurve.timeMax = 0.2f;

                Destroy(areaIndicator.transform.Find("Point Light").gameObject);

                areaIndicator.transform.SetParent(null);
                areaIndicator.gameObject.SetActive(true);

                EffectManager.SpawnEffect(mineDestructionExplosion, new EffectData
                {
                    origin = base.transform.position,
                    scale = 5f
                }, false);
            }
        }

        public class EnterReloadLightSnipe : BaseState
        {
            private float duration
            {
                get
                {
                    return 0.5f / attackSpeedStat;
                }
            }

            public override void OnEnter()
            {
                base.OnEnter();
                PlayAnimation("Gesture, Override", "FirePistol", "FirePistol.playbackRate", duration);
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (!isAuthority || fixedAge < duration)
                {
                    return;
                }
                outer.SetNextState(new ReloadLightSnipe());
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Any;
        }

        public class ReloadLightSnipe : BaseState
        {
            private float duration
            {
                get
                {
                    return 0.425f / attackSpeedStat;
                }
            }

            public override void OnEnter()
            {
                base.OnEnter();
                PlayAnimation("Gesture, Override", "ChargeSuper", "Super.playbackRate", duration * 2f);
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (fixedAge >= duration / 2f)
                {
                    GiveStock();
                }
                if (!isAuthority || fixedAge < duration)
                {
                    return;
                }
                if (skillLocator.secondary.stock < skillLocator.secondary.maxStock)
                {
                    outer.SetNextState(new ReloadLightSnipe());
                    return;
                }
                outer.SetNextState(new ExitReloadLightSnipe());
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            private void GiveStock()
            {
                if (hasGivenStock)
                {
                    return;
                }
                skillLocator.secondary.AddOneStock();
                if (skillLocator.primary.skillDef.skillName == "SnipeLight")
                {
                    skillLocator.primary.AddOneStock();
                }
                Util.PlaySound("Play_railgunner_m2_reload_fail", base.gameObject);
                hasGivenStock = true;
            }

            public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Any;

            private bool hasGivenStock;
        }

        public class ExitReloadLightSnipe : BaseState
        {
            private float duration
            {
                get
                {
                    return 0.25f / attackSpeedStat;
                }
            }

            public override void OnEnter()
            {
                base.OnEnter();
                PlayAnimation("Gesture, Override", "FireSuper", "Super.playbackRate", duration);
                Util.PlaySound("Play_railgunner_m2_reload_pass", base.gameObject);
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (!isAuthority || fixedAge < duration)
                {
                    return;
                }
                outer.SetNextStateToMain();
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Any;
        }

        /*public class ReloadRailgunSkillDef : RailgunSkillDef
  {
      public SerializableEntityStateType reloadState;
      public InterruptPriority reloadInterruptPriority = InterruptPriority.Skill;

      public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
      {
          return new ReloadInstanceData 
          {
              backpackStateMachine = EntityStateMachine.FindByCustomName(skillSlot.gameObject, "Backpack"),
              reloadStateMachine = EntityStateMachine.FindByCustomName(skillSlot.gameObject, "Reload"),
              weaponStateMachine = EntityStateMachine.FindByCustomName(skillSlot.gameObject, "Weapon"),
          };
      }

      public override void OnFixedUpdate([NotNull] GenericSkill skillSlot)
      {
          base.OnFixedUpdate(skillSlot);
          if (skillSlot.stock < GetMaxStock(skillSlot))
          {
              ReloadInstanceData instanceData = (ReloadInstanceData)skillSlot.skillInstanceData;
              if (instanceData.weaponStateMachine && !instanceData.weaponStateMachine.HasPendingState() && instanceData.weaponStateMachine.CanInterruptState(reloadInterruptPriority))
              {
                  instanceData.weaponStateMachine.SetNextState(EntityStateCatalog.InstantiateState(reloadState));
              }
          }
      }

      private class ReloadInstanceData : InstanceData
      {
          public EntityStateMachine weaponStateMachine;
      }
  }*/
        /*RailgunSkillDef scopeLight = Addressables.LoadAssetAsync<RailgunSkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyScopeLight.asset").WaitForCompletion();
            (ScopeLight as ScriptableObject).name = (scopeLight as ScriptableObject).name;
            ScopeLight.skillName = scopeLight.skillName;
            ScopeLight.skillNameToken = scopeLight.skillNameToken;
            ScopeLight.skillDescriptionToken = scopeLight.skillDescriptionToken;
            ScopeLight.keywordTokens = new[] { "KEYWORD_WEAKPOINT", "GS_KEYWORD_PASSIVERELOAD" };
            ScopeLight.icon = scopeLight.icon;
            ScopeLight.offlineIcon = scopeLight.offlineIcon;
            ScopeLight.activationStateMachineName = scopeLight.activationStateMachineName;
            ScopeLight.activationState = scopeLight.activationState;
            ScopeLight.interruptPriority = scopeLight.interruptPriority;
            ScopeLight.reloadInterruptPriority = InterruptPriority.Any;
            ScopeLight.baseRechargeInterval = 1;
            ScopeLight.baseMaxStock = 12;
            ScopeLight.rechargeStock = 0;
            ScopeLight.requiredStock = 0;
            ScopeLight.stockToConsume = 0;
            ScopeLight.resetCooldownTimerOnUse = scopeLight.resetCooldownTimerOnUse;
            ScopeLight.fullRestockOnAssign = scopeLight.fullRestockOnAssign;
            ScopeLight.dontAllowPastMaxStocks = scopeLight.dontAllowPastMaxStocks;
            ScopeLight.beginSkillCooldownOnSkillEnd = scopeLight.beginSkillCooldownOnSkillEnd;
            ScopeLight.cancelSprintingOnActivation = scopeLight.cancelSprintingOnActivation;
            ScopeLight.forceSprintDuringState = scopeLight.forceSprintDuringState;
            ScopeLight.canceledFromSprinting = scopeLight.canceledFromSprinting;
            ScopeLight.isCombatSkill = scopeLight.isCombatSkill;
            ScopeLight.mustKeyPress = scopeLight.mustKeyPress;
            ScopeLight.restockOnReload = false;
            ScopeLight.reloadState = new SerializableEntityStateType(typeof(ReloadLightSnipe));
            ContentAddition.AddSkillDef(ScopeLight);*/
        /*Addressables.LoadAssetAsync<SkillFamily>("RoR2/DLC1/Railgunner/RailgunnerBodySecondaryFamily.asset").Completed += handle =>
{
    int index = Array.FindIndex(handle.Result.variants, x => x.skillDef == scopeLight);
    if (index >= 0)
    {
        handle.Result.variants[index].skillDef = ScopeLight;
    }
};*/
    }
}
