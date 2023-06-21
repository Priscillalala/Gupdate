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

namespace Gupdate.Gameplay.Items
{
    public class Bands : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_ICERING_DESC", "Hits that deal <style=cIsDamage>more than 400% damage</style> also blast enemies with a <style=cIsDamage>runic ice blast</style>, <style=cIsUtility>slowing</style> them by <style=cIsUtility>80%</style> for <style=cIsUtility>3s</style> <style=cStack>(+3s per stack)</style> and dealing <style=cIsDamage>200%</style> <style=cStack>(+200% per stack)</style> TOTAL damage. Recharges every <style=cIsUtility>10</style> seconds."),
            ("ITEM_FIRERING_DESC", "Hits that deal <style=cIsDamage>more than 400% damage</style> also blast enemies with a <style=cIsDamage>runic flame tornado</style>, <style=cIsDamage>igniting</style> them and dealing <style=cIsDamage>150%</style> <style=cStack>(+150% per stack)</style> TOTAL damage over time. Recharges every <style=cIsUtility>10</style> seconds."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElementalRings/FireTornado.prefab").Completed += handle =>
            {
                handle.Result.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;
            };

            IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locIceStackIndex = -1;
            int locFireStackIndex = -1;
            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(RoR2Content), nameof(RoR2Content.Items.IceRing), out locIceStackIndex) 
                && Gutil.TryGotoStackLocIndex(c, typeof(RoR2Content), nameof(RoR2Content.Items.FireRing), out locFireStackIndex))
            {
                ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(locIceStackIndex),
                x => x.MatchConvR4(),
                x => x.MatchMul(),
                x => x.MatchStloc(out _)
                );

                if (ilfound)
                {
                    c.Next.Operand = 2f;
                }

                ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(locFireStackIndex),
                x => x.MatchConvR4(),
                x => x.MatchMul(),
                x => x.MatchStloc(out _)
                );

                if (ilfound)
                {
                    c.Next.Operand = 1.5f;
                }
            }
        }
    }
}
