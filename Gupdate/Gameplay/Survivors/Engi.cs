using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using EntityStates.Engi.Mine;
using EntityStates.Engi.SpiderMine;
using EntityStates.Engi.EngiWeapon;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.Skills;
using RoR2.CharacterAI;
using System.Linq;
using EntityStates;

namespace Gupdate.Gameplay.Monsters
{
    public class Engi : ModBehaviour
    {
        private static Color commonStage2color = new Color32(255, 191, 0, 255);
        private static Color commonStage3color = new Color32(255, 56, 0, 255);

        public override (string, string)[] GetLang() => new[]
        {
            ("ENGI_SPIDERMINE_DESCRIPTION", "Place a robot mine to <style=cIsUtility>hunt enemies</style> and explode repeatedly for <style=cIsDamage>3x200% damage</style>. Can place up to 4."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Engi/EntityStates.Engi.Mine.MineArmingWeak.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(BaseMineArmingState.blastRadiusScale), 0.4f);
            };

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Engi/EntityStates.Engi.EngiWeapon.ChargeGrenades.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(ChargeGrenades.baseTotalDuration), 3 - (2 / 7));
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBubbleShield.prefab").Completed += handle =>
            {
                handle.Result.transform.Find("Collision").localScale = Vector3.one * 22f;
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiWalkerTurretMaster.prefab").Completed += handle =>
            {
                AISkillDriver chaseAndFireAtEnemy = handle.Result.GetComponents<AISkillDriver>().FirstOrDefault(x => x.customName == "ChaseAndFireAtEnemy");
                if (chaseAndFireAtEnemy != null)
                {
                    chaseAndFireAtEnemy.shouldSprint = true;
                }
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/SpiderMine.prefab").Completed += handle =>
            {
                handle.Result.AddComponent<SpiderMineController>();
                //handle.Result.transform.Find("GhostAnchor").localPosition = new Vector3(0f, 0f, -0.4f);
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/SpiderMineGhost.prefab").Completed += handle =>
            {
                SpiderMineGhostInfo ghostInfo = handle.Result.AddComponent<SpiderMineGhostInfo>();
                Material matEngi = Addressables.LoadAssetAsync<Material>("RoR2/Base/Engi/matEngi.mat").WaitForCompletion();
                ghostInfo.stage2material = Instantiate(matEngi);
                ghostInfo.stage2material.SetTexture("_MainTex", Gupdate.assets.LoadAsset<Texture>("texEngiDiffuse2"));
                ghostInfo.stage2material.SetTexture("_EmTex", Gupdate.assets.LoadAsset<Texture>("texEngiEmissionGrayscale"));
                ghostInfo.stage2material.SetColor("_EmColor", commonStage2color);
                ghostInfo.stage3material = Instantiate(matEngi);
                ghostInfo.stage3material.SetTexture("_MainTex", Gupdate.assets.LoadAsset<Texture>("texEngiDiffuse3"));
                ghostInfo.stage3material.SetTexture("_EmTex", Gupdate.assets.LoadAsset<Texture>("texEngiEmissionGrayscale"));
                ghostInfo.stage3material.SetColor("_EmColor", commonStage3color);
                ghostInfo.stage2color = commonStage2color;
                ghostInfo.stage3color = commonStage3color;
                //handle.Result.transform.Find("mdlEngiSpiderMine").localScale = Vector3.one * 0.9f;
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/SpiderMineGhost2.prefab").Completed += handle =>
            {
                SpiderMineGhostInfo ghostInfo = handle.Result.AddComponent<SpiderMineGhostInfo>();
                Material matEngiAlt = Addressables.LoadAssetAsync<Material>("RoR2/Base/Engi/matEngiAlt.mat").WaitForCompletion();
                ghostInfo.stage2material = Instantiate(matEngiAlt);
                ghostInfo.stage2material.SetTexture("_MainTex", Gupdate.assets.LoadAsset<Texture>("texEngiDiffuseAlt2"));
                ghostInfo.stage2material.SetTexture("_EmTex", Gupdate.assets.LoadAsset<Texture>("texEngiEmissionGrayscale"));
                ghostInfo.stage2material.SetColor("_EmColor", commonStage2color);
                ghostInfo.stage3material = Instantiate(matEngiAlt);
                ghostInfo.stage3material.SetTexture("_MainTex", Gupdate.assets.LoadAsset<Texture>("texEngiDiffuseAlt3"));
                ghostInfo.stage3material.SetTexture("_EmTex", Gupdate.assets.LoadAsset<Texture>("texEngiEmissionGrayscale"));
                ghostInfo.stage3material.SetColor("_EmColor", commonStage3color);
                ghostInfo.stage2color = commonStage2color;
                ghostInfo.stage3color = commonStage3color;
            };
            /*Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiBodyPlaceSpiderMine.asset").Completed += handle =>
            {
                handle.Result.baseMaxStock = 8;
                handle.Result.baseRechargeInterval = 4f;
            };*/
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Engi/EntityStates.Engi.EngiWeapon.FireSpiderMine.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(FireSpiderMine.damageCoefficient), 2f);
                handle.Result.TryModifyFieldValue(nameof(FireSpiderMine.force), 400f);
            };
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/Base/Engi/EntityStates.Engi.SpiderMine.Detonate.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(EntityStates.Engi.SpiderMine.Detonate.blastRadius), 8f);
            };

            On.EntityStates.Engi.SpiderMine.WaitForTarget.OnEnter += WaitForTarget_OnEnter;
            IL.EntityStates.Engi.SpiderMine.Detonate.OnEnter += Detonate_OnEnter;
            On.EntityStates.Engi.EngiWeapon.ChargeGrenades.OnEnter += ChargeGrenades_OnEnter;
            IL.EntityStates.Engi.EngiWeapon.ChargeGrenades.FixedUpdate += ChargeGrenades_FixedUpdate;
        }

        private void WaitForTarget_OnEnter(On.EntityStates.Engi.SpiderMine.WaitForTarget.orig_OnEnter orig, EntityStates.Engi.SpiderMine.WaitForTarget self)
        {
            if (self.gameObject.TryGetComponent(out SpiderMineController spiderMineController) && !spiderMineController.ShouldPlayArmedSFX())
            {
                self.enterSoundString = string.Empty;
            }
            orig(self);
        }

        private void Detonate_OnEnter(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.Emit(OpCodes.Ldarg, 0);
            c.EmitDelegate<Action<EntityStates.Engi.SpiderMine.Detonate>>(detonate =>
            {
                if (detonate.gameObject.TryGetComponent(out SpiderMineController spiderMineController))
                {
                    spiderMineController.OnDetonation(detonate);
                }
            });

            /*ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcI4(out _),
                x => x.MatchStfld<BlastAttack>(nameof(BlastAttack.falloffModel))
                );
            if (ilfound)
            {
                c.Next.OpCode = OpCodes.Ldc_I4;
                c.Next.Operand = (int)BlastAttack.FalloffModel.SweetSpot;
            }*/

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EntityState>("get_gameObject"),
                x => x.MatchCallOrCallvirt<EntityState>(nameof(EntityState.Destroy))
                );
            if (ilfound)
            {
                c.Emit(OpCodes.Ret);
            }
        }

        private void ChargeGrenades_OnEnter(On.EntityStates.Engi.EngiWeapon.ChargeGrenades.orig_OnEnter orig, ChargeGrenades self)
        {
            orig(self);
            self.FixedUpdate();
        }

        private void ChargeGrenades_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld<ChargeGrenades>(nameof(ChargeGrenades.maxCharges)),
                x => x.MatchCallOrCallvirt<Mathf>(nameof(Mathf.Min))
                );
            if (ilfound)
            {
                c.Emit(OpCodes.Ldc_I4, 1);
                c.Emit(OpCodes.Add);
            }
        }

        public class SpiderMineGhostInfo : MonoBehaviour
        {
            public Material stage2material;
            public Material stage3material;
            public Color stage2color;
            public Color stage3color;
        }

        public class SpiderMineController : MonoBehaviour
        {
            private int detonationCount;
            private bool hasPlayedArmedSFX;
            private ProjectileController projectileController;

            public void Awake()
            {
                projectileController = base.GetComponent<ProjectileController>();
            }

            public bool ShouldPlayArmedSFX()
            {
                if (!hasPlayedArmedSFX)
                {
                    hasPlayedArmedSFX = true;
                    return true;
                }
                return false;
            }

            public void OnDetonation(EntityStates.Engi.SpiderMine.Detonate detonate)
            {
                detonationCount++;
                if (detonationCount >= 3 || detonate.gameObject.GetComponent<Deployable>().ownerMaster == null)
                {
                    Util.PlaySound("Play_engi_M1_explo", detonate.gameObject);
                    if (NetworkServer.active)
                    {
                        Destroy(base.gameObject);
                    }
                } 
                else 
                {
                    if (detonate.isAuthority)
                    {
                        detonate.outer.SetNextState(new EntityStates.Engi.SpiderMine.WaitForStick());
                    }
                    Transform preDetonate = detonate.FindModelChild("PreDetonate");
                    if (preDetonate)
                    {
                        preDetonate.gameObject.SetActive(false);
                    }
                    SpiderMineGhostInfo ghostInfo = projectileController?.ghost?.GetComponent<SpiderMineGhostInfo>();
                    if (ghostInfo)
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = projectileController.ghost.GetComponentInChildren<SkinnedMeshRenderer>();
                        if (skinnedMeshRenderer)
                        {
                            switch (detonationCount)
                            {
                                case 1:
                                    skinnedMeshRenderer.sharedMaterial = ghostInfo.stage2material;
                                    break;
                                case 2:
                                    skinnedMeshRenderer.sharedMaterial = ghostInfo.stage3material;
                                    break;
                            }
                        }
                        foreach (Light light in projectileController.ghost.GetComponentsInChildren<Light>())
                        {
                            switch (detonationCount)
                            {
                                case 1:
                                    light.color = ghostInfo.stage2color;
                                    break;
                                case 2:
                                    light.color = ghostInfo.stage3color;
                                    break;
                            }
                        }
                        Transform armed = detonate.FindModelChild("Armed");
                        if (armed && armed.TryGetComponent(out LineRenderer armedLineRenderer))
                        {
                            switch (detonationCount)
                            {
                                case 1:
                                    armedLineRenderer.startColor = ghostInfo.stage2color;
                                    break;
                                case 2:
                                    armedLineRenderer.startColor = ghostInfo.stage3color;
                                    break;
                            }
                        }
                        Transform chase = detonate.FindModelChild("Chase");
                        TrailRenderer chaseTrailRenderer = chase.GetComponentInChildren<TrailRenderer>();
                        if (chase && chaseTrailRenderer && chase.TryGetComponent(out LineRenderer chaseLineRenderer))
                        {
                            switch (detonationCount)
                            {
                                case 1:
                                    chaseTrailRenderer.startColor = ghostInfo.stage2color;
                                    chaseLineRenderer.startColor = ghostInfo.stage2color;
                                    break;
                                case 2:
                                    chaseTrailRenderer.startColor = ghostInfo.stage3color;
                                    chaseLineRenderer.startColor = ghostInfo.stage3color;
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
