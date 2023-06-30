using System;
using System.Collections.Generic;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using EntityStates.VoidCamp;
using UnityEngine.Rendering.PostProcessing;
using RoR2.UI;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections;
using HG;

namespace Gupdate.Gameplay
{
    public class VoidCampStages : ModBehaviour
    {
        private GameObject voidCampPrefab;
        public static bool VoidStageActive { get; private set; }
        private static readonly HashSet<SceneIndex> awaitingVoidStages = new HashSet<SceneIndex>();
        private static GameObject voidCampStageWeather;

        public override (string, string)[] GetLang() => new[]
        {
            ("GS_MAP_VOIDCAMPSTAGE_SUBTITLE", "Void Seed VII"),
            ("GS_VOIDCAMPSTAGE_COMPLETE", "<style=cWorldEvent>The Void recedes..</style>"),
        };

        public void Awake()
        {
            voidCampPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidCamp/VoidCamp.prefab").WaitForCompletion();
            voidCampPrefab.AddComponent<MarkStageAsAwaitingVoid>();

            voidCampStageWeather = new GameObject().InstantiateClone("Weather, VoidCampStage", true);
            voidCampStageWeather.AddComponent<NetworkIdentity>();

            GameObject pp = new GameObject("PP");
            pp.transform.SetParent(voidCampStageWeather.transform);
            pp.layer = 20;
            PostProcessVolume postProcessVolume = pp.AddComponent<PostProcessVolume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 10f;
            postProcessVolume.weight = 0.95f;
            postProcessVolume.sharedProfile = Instantiate(Addressables.LoadAssetAsync<PostProcessProfile>("RoR2/Base/Common/ppLocalVoidFogMild.asset").WaitForCompletion());
            postProcessVolume.sharedProfile.GetSetting<ColorGrading>().mixerRedOutRedIn.Override(85f);
            postProcessVolume.sharedProfile.GetSetting<RampFog>().skyboxStrength.Override(0.1f);
            PostProcessDuration postProcessDuration = pp.AddComponent<PostProcessDuration>();
            postProcessDuration.enabled = false;
            postProcessDuration.maxDuration = 8f;
            postProcessDuration.ppWeightCurve = AnimationCurve.EaseInOut(0f, 0.95f, 1f, 0f);
            postProcessDuration.ppVolume = postProcessVolume;

            GameObject fog = voidCampPrefab.transform.Find("mdlVoidFogEmitter/RangeIndicator/RangeFX").gameObject.InstantiateClone("Fog", false);
            fog.transform.SetParent(voidCampStageWeather.transform, false);
            MeshRenderer fogMeshRenderer = fog.GetComponent<MeshRenderer>();
            fogMeshRenderer.SetSharedMaterials(new[] { Addressables.LoadAssetAsync<Material>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/matInfiniteTowerSkyboxSphere, Scrolling Caustics.mat").WaitForCompletion() }, 1);
            WeatherParticles weatherParticles = fog.AddComponent<WeatherParticles>();
            weatherParticles.lockPosition = true;
            weatherParticles.lockRotation = false;
            weatherParticles.resetPositionToZero = true;
            AnimateShaderAlpha animateShaderAlpha = fog.GetComponent<AnimateShaderAlpha>();
            animateShaderAlpha.timeMax = 8f;
            fog.transform.localScale = Vector3.one * 1000f;

            WeatherController weatherController = voidCampStageWeather.AddComponent<WeatherController>();
            weatherController.postProcessDuration = postProcessDuration;
            weatherController.animateShaderAlpha = animateShaderAlpha;

            //ContentAddition.AddNetworkedObject(voidStagePP);
            PrefabAPI.RegisterNetworkPrefab(voidCampStageWeather);

            On.RoR2.TeleporterInteraction.AttemptToSpawnAllEligiblePortals += TeleporterInteraction_AttemptToSpawnAllEligiblePortals;
            On.RoR2.TeleporterInteraction.Start += TeleporterInteraction_Start;
            On.RoR2.ClassicStageInfo.Awake += ClassicStageInfo_Awake;
            On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
            IL.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;
            On.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;
            On.RoR2.SceneDirector.GenerateInteractableCardSelection += SceneDirector_GenerateInteractableCardSelection;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
            On.RoR2.UI.AssignStageToken.Start += AssignStageToken_Start;
            SceneDirector.onPrePopulateSceneServer += SceneDirector_onPrePopulateSceneServer;
            SceneCatalog.onMostRecentSceneDefChanged += SceneCatalog_onMostRecentSceneDefChanged;
            On.EntityStates.VoidCamp.Deactivate.OnEnter += Deactivate_OnEnter;
            SceneDirector.onGenerateInteractableCardSelection += SceneDirector_onGenerateInteractableCardSelection;
        }

        private void TeleporterInteraction_AttemptToSpawnAllEligiblePortals(On.RoR2.TeleporterInteraction.orig_AttemptToSpawnAllEligiblePortals orig, TeleporterInteraction self)
        {
            orig(self);
            if (NetworkServer.active && VoidStageActive)
            {
                self.AttemptSpawnPortal(Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/PortalVoid/iscVoidPortal.asset").WaitForCompletion(), 10f, 40f, "PORTAL_VOID_OPEN");
            }
        }

        private void TeleporterInteraction_Start(On.RoR2.TeleporterInteraction.orig_Start orig, TeleporterInteraction self)
        {
            SpawnCard iscVoidPortal = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/PortalVoid/iscVoidPortal.asset").WaitForCompletion();
            for (int i = self.portalSpawners.Length - 1; i >= 0; i--)
            {
                if (self.portalSpawners[i].portalSpawnCard == iscVoidPortal)
                {
                    DestroyImmediate(self.portalSpawners[i]);
                    ArrayUtils.ArrayRemoveAtAndResize(ref self.portalSpawners, i);
                }
            }
            orig(self);
            if (VoidStageActive)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "PORTAL_VOID_WILL_OPEN"
                });
                if (self.modelChildLocator)
                {
                    self.modelChildLocator.FindChild("VoidPortalIndicator").gameObject.SetActive(true);
                }
            }
        }

        private void ClassicStageInfo_Awake(On.RoR2.ClassicStageInfo.orig_Awake orig, ClassicStageInfo self)
        {
            if (VoidStageActive)
            {
                self.sceneDirectorMonsterCredits = (int)(self.sceneDirectorMonsterCredits * 1.5f);
            }
            orig(self);
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            if (VoidStageActive)
            {
                self.creditMultiplier *= 1.5f;
            }
            orig(self);
        }

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            On.RoR2.CombatDirector.Spawn += AddVoidElites;
            orig(self);
            On.RoR2.CombatDirector.Spawn -= AddVoidElites;
        }

        private bool AddVoidElites(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            if (VoidStageActive && !eliteDef)
            {
                CharacterBody body = spawnCard?.prefab?.GetComponent<CharacterMaster>()?.bodyPrefab?.GetComponent<CharacterBody>();
                bool isVoidMonster = body && (body.bodyFlags & CharacterBody.BodyFlags.Void) > CharacterBody.BodyFlags.None;
                if (!isVoidMonster && self.rng.nextNormalizedFloat <= 0.5f) 
                {
                    eliteDef = DLC1Content.Elites.Void;
                }
            }
            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }

        private void CombatDirector_Spawn(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchStfld<DirectorSpawnRequest>(nameof(DirectorSpawnRequest.teamIndexOverride))
                );

            if (ilfound)
            {
                c.Emit(OpCodes.Ldarg, 1);
                c.Emit(OpCodes.Ldarg, 2);
                c.EmitDelegate<Func<TeamIndex?, SpawnCard, EliteDef, TeamIndex?>>((teamIndex, spawnCard, eliteDef) =>
                {
                    if (!VoidStageActive)
                    {
                        return teamIndex;
                    }
                    bool isVoidElite = eliteDef && eliteDef == DLC1Content.Elites.Void;
                    CharacterBody body = spawnCard?.prefab?.GetComponent<CharacterMaster>()?.bodyPrefab?.GetComponent<CharacterBody>();
                    bool isVoidMonster = body && (body.bodyFlags & CharacterBody.BodyFlags.Void) > CharacterBody.BodyFlags.None;
                    if (isVoidElite || isVoidMonster)
                    {
                        return TeamIndex.Void;
                    }
                    return teamIndex;
                });
            }
        }

        private void ClassicStageInfo_RebuildCards(On.RoR2.ClassicStageInfo.orig_RebuildCards orig, ClassicStageInfo self)
        {
            orig(self);
            if (VoidStageActive)
            {
                AddToDccsDeck(self.monsterSelection, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/VoidCamp/dccsVoidCampMonsters.asset").WaitForCompletion(), 1f);
            }
        }

        private WeightedSelection<DirectorCard> SceneDirector_GenerateInteractableCardSelection(On.RoR2.SceneDirector.orig_GenerateInteractableCardSelection orig, SceneDirector self)
        {
            WeightedSelection<DirectorCard> result = orig(self);
            if (VoidStageActive)
            {
                AddToDccsDeck(result, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/VoidCamp/dccsVoidCampInteractables.asset").WaitForCompletion(), 1f);
            }
            return result;
        }

        public static void AddToDccsDeck(WeightedSelection<DirectorCard> deck, DirectorCardCategorySelection newDccs, float newDccsWeight)
        {
            WeightedSelection<DirectorCard> other = newDccs.GenerateDirectorCardWeightedSelection();
            float expectedWeight = deck.totalWeight * newDccsWeight;
            float weightCoefficient = expectedWeight / other.totalWeight;
            for (int i = 0; i < other.Count; i++)
            {
                WeightedSelection<DirectorCard>.ChoiceInfo choice = other.GetChoice(i);
                choice.weight *= weightCoefficient;
                deck.AddChoice(choice);
            }
        }

        private void Run_onRunDestroyGlobal(Run _) => awaitingVoidStages.Clear();

        private void AssignStageToken_Start(On.RoR2.UI.AssignStageToken.orig_Start orig, AssignStageToken self)
        {
            orig(self);
            if (VoidStageActive)
            {
                self.subtitleText.SetText(Language.GetString("GS_MAP_VOIDCAMPSTAGE_SUBTITLE"), true);
            }
        }

        private void SceneDirector_onPrePopulateSceneServer(SceneDirector sceneDirector)
        {
            if (!VoidStageActive)
            {
                return;
            }
            sceneDirector.onPopulateCreditMultiplier *= 1.5f;
            DirectorCardCategorySelection props = Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/VoidCamp/dccsVoidCampFlavorProps.asset").WaitForCompletion();
            WeightedSelection<DirectorCard> propDeck = props.GenerateDirectorCardWeightedSelection();
            Xoroshiro128Plus rng = new Xoroshiro128Plus(sceneDirector.rng.nextUlong);
            int propCredits = (int)(sceneDirector.interactableCredit * 2);
            while (propCredits > 0)
            {
                DirectorCard directorCard = sceneDirector.SelectCard(propDeck, propCredits);
                if (directorCard == null)
                {
                    break;
                }
                if (directorCard.IsAvailable())
                {
                    propCredits -= directorCard.cost;
                    if (Run.instance)
                    {
                        int i = 0;
                        while (i < 10)
                        {
                            DirectorPlacementRule placementRule = new DirectorPlacementRule
                            {
                                placementMode = DirectorPlacementRule.PlacementMode.Random
                            };
                            if (DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(directorCard.spawnCard, placementRule, rng))) 
                            {
                                break;
                            } 
                            else
                            {
                                i++;
                            }
                        }
                    }
                }
            }
        }

        private void SceneCatalog_onMostRecentSceneDefChanged(SceneDef mostRecentSceneDef)
        {
            VoidStageActive = awaitingVoidStages.Remove(mostRecentSceneDef.sceneDefIndex);

            if (VoidStageActive && NetworkServer.active)
            {
                GameObject instance = Instantiate(voidCampStageWeather);
                NetworkServer.Spawn(instance);
            }
        }

        private void Deactivate_OnEnter(On.EntityStates.VoidCamp.Deactivate.orig_OnEnter orig, Deactivate self)
        {
            awaitingVoidStages.Remove(SceneCatalog.mostRecentSceneDef.sceneDefIndex);
            orig(self);
        }

        private void SceneDirector_onGenerateInteractableCardSelection(SceneDirector sceneDirector, DirectorCardCategorySelection dccs)
        {
            if (VoidStageActive)
            {
                dccs.RemoveCardsThatFailFilter(x => x.spawnCard?.prefab != voidCampPrefab);
            }
        }

        public class MarkStageAsAwaitingVoid : MonoBehaviour
        {
            public void Start()
            {
                awaitingVoidStages.Add(SceneCatalog.mostRecentSceneDef.sceneDefIndex);
                Destroy(this);
            }
        }

        public class WeatherController : MonoBehaviour
        {
            public AnimateShaderAlpha animateShaderAlpha;
            public PostProcessDuration postProcessDuration;

            public void OnEnable()
            {
                TeleporterInteraction.onTeleporterChargedGlobal += TeleporterInteraction_onTeleporterChargedGlobal;
            }

            public void OnDisable()
            {
                TeleporterInteraction.onTeleporterChargedGlobal -= TeleporterInteraction_onTeleporterChargedGlobal;
            }

            private void TeleporterInteraction_onTeleporterChargedGlobal(TeleporterInteraction obj)
            {
                base.StartCoroutine(nameof(BroadcastCompletion));
                base.gameObject.AddComponent<DestroyOnTimer>().duration = 8f;
                animateShaderAlpha.enabled = true;
                postProcessDuration.enabled = true;
            }

            public IEnumerator BroadcastCompletion()
            {
                yield return new WaitForSeconds(1f);
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "GS_VOIDCAMPSTAGE_COMPLETE"
                });
            }
        }
    }
}
