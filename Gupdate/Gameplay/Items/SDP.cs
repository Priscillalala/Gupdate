using System;
using BepInEx;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.Items;
using Mono.Cecil.Cil;
using EntityStates.DroneWeaponsChainGun;

namespace Gupdate.Gameplay.Items
{
    public class SDP : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_DRONEWEAPONS_PICKUP", "Your drones shoot missiles, gain a bonus chaingun, and attack more frequently."),
            ("ITEM_DRONEWEAPONS_DESC", "Gain <style=cIsDamage>Col. Droneman</style>. \nDrones gain a <style=cIsDamage>10%</style> chance to fire a <style=cIsDamage>missile</style> on hit, dealing <style=cIsDamage>300%</style> TOTAL damage. \nDrones gain an <style=cIsDamage>automatic chain gun</style> that deals <style=cIsDamage>6x100%</style>damage.\nDrones gain <style=cIsUtility>+50%</style> <style=cStack>(+50% per stack)</style> cooldown reduction. "),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/DLC1/DroneWeapons/EntityStates.DroneWeaponsChainGun.FireChainGun.asset").Completed += handle =>
            {
                handle.Result.TryModifyFieldValue(nameof(FireChainGun.additionalBounces), 0);
            };

            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(DLC1Content), nameof(DLC1Content.Items.DroneWeaponsBoost), out int locStackIndex))
            {
                ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(locStackIndex),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(out _),
                x => x.MatchMul(),
                x => x.MatchAdd()
                );

                if (ilfound)
                {
                    c.Next.Next.Next.Operand = 0f;
                }
            }
        }
    }
}
