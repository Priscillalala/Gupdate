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

namespace Gupdate.Gameplay.Monsters
{
    public class VoidLocus : ModBehaviour
    {
        public void Awake()
        {
            /*Addressables.LoadAssetAsync<Material>("RoR2/DLC1/snowyforest/matSFAurora.mat").Completed += handle =>
            {
                handle.Result.SetColor("_TintColor", new Color32(207, 0, 140, 255));
                handle.Result.SetFloat("_Boost", 2f);
                handle.Result.SetFloat("_AlphaBoost", 0.15f);
            };*/

            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(Scene oldScene, Scene newScene)
        {
            if (newScene.name == "voidstage")
            {
                GameObject terrain = GameObject.Find("HOLDER: Terrain");
                if (terrain && terrain.transform.TryFind("OLDTERRAIN", out Transform oldTerrain))
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
