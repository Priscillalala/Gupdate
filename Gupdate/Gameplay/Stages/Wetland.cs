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

namespace Gupdate.Gameplay.Monsters
{
    public class Wetland : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<PostProcessProfile>("RoR2/Base/title/ppSceneFoggyswamp.asset").Completed += handle =>
            {
                if (handle.Result.TryGetSettings(out RampFog rampFog))
                {
                    rampFog.fogZero.Override(0f);
                    rampFog.fogOne.Override(0.4f);
                    rampFog.fogColorStart.Override(new Color32(111, 132, 124, 20));
                    rampFog.fogColorMid.Override(new Color32(76, 97, 92, 206));
                }
            };
        }
    }
}
