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

namespace Gupdate.Gameplay.Monsters
{
    public class Moon : ModBehaviour
    {
        public void Awake()
        {
            On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            if (SceneCatalog.mostRecentSceneDef.sceneDefIndex == SceneCatalog.FindSceneIndex("moon2") && self.teamIndex == TeamIndex.Monster)
            {
                self.goldRewardCoefficient = 0;
            }
            orig(self);
        }
    }
}
