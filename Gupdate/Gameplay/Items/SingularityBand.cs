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
using HG;

namespace Gupdate.Gameplay.Items
{
    public class SingularityBand : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_ELEMENTALRINGVOID_DESC", "Hits that deal <style=cIsDamage>more than 400% damage</style> also fire a black hole that <style=cIsUtility>draws enemies within 15m into its center</style>. Lasts <style=cIsUtility>5</style> seconds before collapsing, dealing <style=cIsDamage>150%</style> <style=cStack>(+200% per stack)</style> TOTAL damage. Recharges every <style=cIsUtility>20</style> seconds. <style=cIsVoid>Corrupts all Runald's and Kjaro's Bands</style>."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ElementalRingVoid/ElementalRingVoidBlackHole.prefab").Completed += handle =>
            {
                handle.Result.GetComponent<ProjectileController>().procCoefficient = 0.5f;
            };

            IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(DLC1Content), nameof(DLC1Content.Items.ElementalRingVoid), out int locStackIndex))
            {
                ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(locStackIndex),
                x => x.MatchConvR4(),
                x => x.MatchMul()
                );

                if (ilfound)
                {
                    c.Next.Operand = 2f;
                    c.Index += 4;
                    c.Emit(OpCodes.Ldc_R4, 0.5f);
                    c.Emit(OpCodes.Sub);
                }
            }
        }

        public class ScaleRadiusWithStacks : MonoBehaviour
        {
            public void Start()
            {
                int stack = 1;
                ProjectileController projectileController = base.GetComponent<ProjectileController>();
                Inventory inventory = projectileController?.owner?.GetComponent<CharacterBody>()?.inventory;
                if (inventory)
                {
                    stack = Mathf.Max(stack, inventory.GetItemCount(DLC1Content.Items.ElementalRingVoid));
                }
                float expectedRadius = Gutil.StackScaling(10f, 2f, stack);
                float radiusAdjustmentCoefficient = expectedRadius / 15f;
                if (base.TryGetComponent(out ProjectileExplosion projectileExplosion))
                {
                    projectileExplosion.blastRadius = expectedRadius;
                }
                if (base.TryGetComponent(out RadialForce radialForce))
                {
                    radialForce.radius = expectedRadius;
                }
                foreach (Transform transform in base.transform.AllChildren())
                {
                    transform.localScale *= radiusAdjustmentCoefficient;
                }
                if (base.transform.TryFind("Point light", out Transform lightTransform) && lightTransform.TryGetComponent(out Light light))
                {
                    light.range *= radiusAdjustmentCoefficient;
                }
                if (base.transform.TryFind("AreaIndicator", out Transform indicatorTransform) && indicatorTransform.TryGetComponent(out ObjectScaleCurve objectScaleCurve))
                {
                    objectScaleCurve.baseScale *= radiusAdjustmentCoefficient;
                }
            }
        }
    }
}
