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
    public class SiphonedForest : ModBehaviour
    {
        public float speedCoefficient = 1.25f;

        public void Awake()
        {
            Addressables.LoadAssetAsync<Material>("RoR2/DLC1/snowyforest/matSFAurora.mat").Completed += handle =>
            {
                handle.Result.SetColor("_TintColor", new Color32(207, 0, 140, 255));
                handle.Result.SetFloat("_Boost", 2f);
                handle.Result.SetFloat("_AlphaBoost", 0.15f);
            };

            IL.RoR2.CharacterMotor.PreMove += CharacterMotor_PreMove;
        }

        private void CharacterMotor_PreMove(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt<Vector3>(nameof(Vector3.MoveTowards)))
                && c.TryGotoPrev(MoveType.After,
                x => x.MatchLdfld<CharacterMotor>(nameof(CharacterMotor.velocity)),
                x => x.MatchLdloc(out _)
                );
            if (ilfound)
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<Vector3, CharacterMotor, Vector3>>((target, motor) =>
                {
                    if (!motor.isAirControlForced)
                    {
                        return target;
                    }
                    return new Vector3(target.x * speedCoefficient, target.y, target.z * speedCoefficient);
                });
            }
        }
    }
}
