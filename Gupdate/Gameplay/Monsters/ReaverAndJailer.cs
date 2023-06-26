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
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine.Events;
using EntityStates.VoidJailer.Weapon;
using Mono.Cecil;

namespace Gupdate.Gameplay.Monsters
{
    public class ReaverAndJailer : ModBehaviour
    {
        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierBody.prefab").Completed += handle =>
            {
                CharacterBody nullifierBody = handle.Result.GetComponent<CharacterBody>();
                nullifierBody.baseNameToken = "VOIDJAILER_BODY_NAME";
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerBody.prefab").Completed += handle =>
            {
                CharacterBody voidJailerBody = handle.Result.GetComponent<CharacterBody>();
                voidJailerBody.baseNameToken = "NULLIFIER_BODY_NAME";
            };

            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/DLC1/VoidJailer/EntityStates.VoidJailer.Weapon.Capture2.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(Capture2.debuffDef), (BuffDef)null);
            };

            IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs).GetField(nameof(RoR2Content.Buffs.NullifyStack))),
                x => x.MatchLdcR4(out _)
                );
            if (ilfound) 
            {
                (c.Next.Operand as FieldReference).Name = nameof(RoR2Content.Buffs.Nullified);
                c.Next.Next.Operand = 2f;
            }
        }
    }
}
