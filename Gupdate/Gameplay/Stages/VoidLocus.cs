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
using UnityEngine.SceneManagement;
using EntityStates.DeepVoidPortalBattery;
using RoR2.UI;
using System.Collections.Generic;

namespace Gupdate.Gameplay.Monsters
{
    public class VoidLocus : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("OBJECTIVE_VOID_BATTERY_MISSION", "Find and activate the <style=cIsVoid>Deep Void Signal</style>"),
            ("OBJECTIVE_VOID_BATTERY", "Charge the <style=cIsVoid>Deep Void Signal</style> ({0}%)"),
            ("OBJECTIVE_VOID_BATTERY_OOB", "Enter the <style=cIsVoid>Deep Void Signal radius!</style> ({0}%)"),
        };

        public static bool HasBatteryActivated { get; private set; }

        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DeepVoidPortalBattery/DeepVoidPortalBattery.prefab").Completed += handle =>
            {
                HoldoutZoneController holdoutZoneController = handle.Result.GetComponent<HoldoutZoneController>();
                holdoutZoneController.baseRadius = 60f;
                holdoutZoneController.chargeRadiusDelta = -50f;
                holdoutZoneController.baseChargeDuration = 90f;
                if (handle.Result.transform.TryFind("Model", out Transform model))
                {
                    model.localScale = Vector3.one * 2f;
                    model.localPosition = new Vector3(0f, -2.7f, 0f);
                    if (model.TryFind("Holder", out Transform holder))
                    {
                        holder.localScale = Vector3.one * 0.2f;
                        holder.localPosition = new Vector3(0f, 1f, 0f);
                    }
                    if (model.TryFind("mdlVoidSignal/IdleFX/Beam, Strong", out Transform beam))
                    {
                        beam.localScale = new Vector3(0.3f, 10f, 0.3f);
                        beam.localPosition = new Vector3(0f, 0f, 0f);
                        /*ObjectScaleCurve objectScaleCurve = beam.gameObject.AddComponent<ObjectScaleCurve>();
                        objectScaleCurve.enabled = false;
                        objectScaleCurve.curveX = AnimationCurve.EaseInOut(0f, 1f, 1f, 5f);
                        objectScaleCurve.curveY = AnimationCurve.Constant(0f, 1f, 1f);
                        objectScaleCurve.curveZ = AnimationCurve.Constant(0f, 1f, 1f);
                        objectScaleCurve.timeMax = 0.2f;
                        objectScaleCurve.useOverallCurveOnly = false;*/
                    }
                }
            };

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/DLC1/DeepVoidPortalBattery/EntityStates.DeepVoidPortalBattery.Idle.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(Idle.onEnterChildToEnable), string.Empty);
            };

            Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/DLC1/DeepVoidPortalBattery/iscDeepVoidPortalBattery.asset").Completed += handle =>
            {
                handle.Result.hullSize = HullClassification.BeetleQueen;
            };

            On.RoR2.VoidStageMissionController.OnCollectObjectiveSources += VoidStageMissionController_OnCollectObjectiveSources;
            On.EntityStates.DeepVoidPortalBattery.Charging.OnEnter += Charging_OnEnter;
            On.EntityStates.DeepVoidPortalBattery.Charged.OnEnter += Charged_OnEnter;
            On.RoR2.VoidStageMissionController.Start += VoidStageMissionController_Start;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void VoidStageMissionController_OnCollectObjectiveSources(On.RoR2.VoidStageMissionController.orig_OnCollectObjectiveSources orig, VoidStageMissionController self, CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
        {
            if (HasBatteryActivated)
            {
                return;
            }
            orig(self, master, objectiveSourcesList);
        }

        private void Charging_OnEnter(On.EntityStates.DeepVoidPortalBattery.Charging.orig_OnEnter orig, Charging self)
        {
            orig(self);
            HasBatteryActivated = true;
        }

        private void Charged_OnEnter(On.EntityStates.DeepVoidPortalBattery.Charged.orig_OnEnter orig, Charged self)
        {
            orig(self);
            /*ObjectScaleCurve beamScale = self.FindModelChildGameObject("IdleFX")?.GetComponentInChildren<ObjectScaleCurve>();
            if (beamScale)
            {
                beamScale.enabled = true;
            }*/
            GameObject idleFx = self.FindModelChildGameObject("IdleFX");
            if (idleFx)
            {
                idleFx.transform.Find("Beam, Strong").localScale = new Vector3(1.1f, 10f, 1.1f);
            }
        }

        private void VoidStageMissionController_Start(On.RoR2.VoidStageMissionController.orig_Start orig, VoidStageMissionController self)
        {
            HasBatteryActivated = false;
            self.batteryCount = 1;
            orig(self);
        }

        private void SceneManager_activeSceneChanged(Scene oldScene, Scene newScene)
        {
            if (newScene.name == "voidstage")
            {
                GameObject terrain = GameObject.Find("HOLDER: Terrain");
                if (terrain)
                {
                    if (terrain.transform.TryFind("OLDTERRAIN", out Transform oldTerrain)) 
                    {
                        oldTerrain.gameObject.SetActive(true);
                        foreach (Transform child in oldTerrain.AllChildren())
                        {
                            child.gameObject.SetActive(false);
                        }
                        if (oldTerrain.transform.TryFind("meshVoidOutterTerrain", out Transform outerTerrainMesh))
                        {
                            outerTerrainMesh.gameObject.SetActive(true);
                            outerTerrainMesh.localScale *= 1.3f;
                        }
                        /*if (oldTerrain.transform.TryFind("meshVoidDistantProp", out Transform distantPropsMesh))
                        {
                            distantPropsMesh.gameObject.SetActive(true);
                            distantPropsMesh.localScale *= 1.5f;
                        }*/
                    }
                    if (terrain.transform.TryFind("Revamped Hopoo Terrain/Islands/mdlVoidStageIslandSE/mdlVoidTerrainSE/mdlVoidArchEntry", out Transform voidArch))
                    {
                        voidArch.gameObject.layer = 11;
                    }
                }
                GameObject weather = GameObject.Find("Weather, Void Stage");
                if (weather)
                {
                    if (weather.transform.TryFind("HOLDER: FX/Blackhole", out Transform blackHole)) 
                    {
                        blackHole.transform.localScale *= 2f;
                        blackHole.transform.localEulerAngles = new Vector3(50f, 0f, 20f);
                        if (blackHole.transform.TryFind("Offset/VoidStageBlackholeMesh", out Transform blackholeMesh))
                        {
                            blackholeMesh.localScale = Vector3.one * 200f;
                        }
                        if (blackHole.transform.TryFind("Offset/Blackhole Center", out Transform blackholeCenter))
                        {
                            blackholeCenter.localScale = Vector3.one * 80f;
                        }
                    }
                    if (weather.transform.TryFind("HOLDER: Cloud Floor/Point Light (2)", out Transform light))
                    {
                        light.gameObject.SetActive(true);
                        light.GetComponent<Light>().intensity = 1;
                    }
                }
            }
        }
    }
}
