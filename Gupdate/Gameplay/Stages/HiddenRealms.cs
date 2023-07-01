using System;
using BepInEx;
using RoR2.CharacterAI;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace Gupdate.Gameplay.Monsters
{
    public class HiddenRealms : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<SceneDef>("RoR2/DLC1/voidstage/voidstage.asset").Completed += handle =>
            {
                handle.Result.sceneType = SceneType.Intermission;
            };
            Addressables.LoadAssetAsync<SceneDef>("RoR2/DLC1/voidraid/voidraid.asset").Completed += handle =>
            {
                handle.Result.sceneType = SceneType.Intermission;
            };
            Addressables.LoadAssetAsync<SceneDef>("RoR2/Base/goldshores/goldshores.asset").Completed += handle =>
            {
                handle.Result.sceneType = SceneType.Stage;
            };
            Addressables.LoadAssetAsync<SceneDef>("RoR2/Base/artifactworld/artifactworld.asset").Completed += handle =>
            {
                handle.Result.sceneType = SceneType.Stage;
            };

            SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
            IL.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            On.RoR2.SceneCatalog.SetSceneDefs += SceneCatalog_SetSceneDefs;
        }

        private void SceneDirector_onPostPopulateSceneServer(SceneDirector obj)
        {
            if (!SceneInfo.instance.countsAsStage && SceneInfo.instance.sceneDef.sceneDefIndex != SceneCatalog.FindSceneIndex("voidstage") && SceneInfo.instance.sceneDef.sceneDefIndex != SceneCatalog.FindSceneIndex("voidraid"))
            {
                return;
            }
            Xoroshiro128Plus rng = new Xoroshiro128Plus(obj.rng.nextUlong);
            int voidKeyCount = 0;
            foreach (CharacterMaster characterMaster in CharacterMaster.readOnlyInstancesList)
            {
                if (characterMaster.inventory.GetItemCount(DLC1Content.Items.TreasureCacheVoid) > 0)
                {
                    voidKeyCount++;
                }
            }
            for (int k = 0; k < voidKeyCount; k++)
            {
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscLockboxVoid"), new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                }, rng));
            }
        }

        private void SceneDirector_PopulateScene(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(DLC1Content.Items).GetField(nameof(DLC1Content.Items.TreasureCacheVoid))),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount))
                );

            if (ilfound)
            {
                c.EmitDelegate<Func<int, int>>(_ => 0);
            }
        }

        private void SceneManager_activeSceneChanged(Scene oldScene, Scene newScene)
        {
            if (newScene.name == "goldshores" && NetworkServer.active)
            {
                GameObject instance = Instantiate(
                    Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldChest/GoldChest.prefab").WaitForCompletion(),
                    new Vector3(-8.5f, 124.6f, -66f),
                    Quaternion.Euler(355f, 250f, 4.33472207f)
                    );
                PurchaseInteraction purchaseInteraction = instance.GetComponent<PurchaseInteraction>();
                purchaseInteraction.automaticallyScaleCostWithDifficulty = true;
                purchaseInteraction.Networkcost = Run.instance.GetDifficultyScaledCost(purchaseInteraction.cost);
                NetworkServer.Spawn(instance);
            }
        }

        private void SceneCatalog_SetSceneDefs(On.RoR2.SceneCatalog.orig_SetSceneDefs orig, SceneDef[] newSceneDefs)
        {
            for (int i = 0; i < newSceneDefs.Length; i++)
            {
                newSceneDefs[i].blockOrbitalSkills = newSceneDefs[i].sceneType != SceneType.Stage;
            }
            orig(newSceneDefs);
        }
    }
}
