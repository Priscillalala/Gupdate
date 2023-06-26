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
using RoR2.Orbs;

namespace Gupdate.Gameplay.Items
{
    public class Polylute : ModBehaviour
    {
        private static GameObject voidLightningStrikeImpact;

        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_CHAINLIGHTNINGVOID_PICKUP", "...and his music was<style=cIsVoid>【el?ectric??.』</style>"),
            ("ITEM_CHAINLIGHTNINGVOID_DESC", "<style=cIsDamage>25%</style> chance to fire <style=cIsDamage>lightning</style> for <style=cIsDamage>60%</style> TOTAL damage up to <style=cIsDamage>2 <style=cStack>(+3 per stack)</style></style> times. <style=cIsVoid>Corrupts all Ukuleles</style>."),
        };

        public void Awake()
        {
            voidLightningStrikeImpact = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ChainLightningVoid/VoidLightningStrikeImpact.prefab").WaitForCompletion()
                .InstantiateClone("VoidLightningStrikeImpactNew", false);
            voidLightningStrikeImpact.transform.Find("Flash").localScale = Vector3.one * 0.7f;
            voidLightningStrikeImpact.transform.Find("OmniSparks").localScale = Vector3.one * 0.7f;
            ContentAddition.AddEffect(voidLightningStrikeImpact);

            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ChainLightningVoid/VoidLightningOrbEffect.prefab").Completed += handle =>
            {
                handle.Result.GetComponent<EffectComponent>().soundName = "Play_item_use_BFG_zaps";
                handle.Result.GetComponent<OrbEffect>().endEffect = voidLightningStrikeImpact;
            };

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXLightning.prefab").Completed += handle =>
            {
                GameObject scaledHitspark = Instantiate(handle.Result.transform.Find("Scaled Hitspark 3 (Random Color)").gameObject, voidLightningStrikeImpact.transform);
                scaledHitspark.transform.localPosition = Vector3.zero;
                scaledHitspark.transform.localScale = Vector3.one * 1.5f;
                scaledHitspark.SetActive(true);
                ParticleSystem particleSystem = scaledHitspark.GetComponent<ParticleSystem>();
                var main = particleSystem.main;
                main.startColor = new ParticleSystem.MinMaxGradient(Color.white);
                scaledHitspark.GetComponent<ParticleSystemRenderer>().sharedMaterial = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitsparkVoid.mat").WaitForCompletion();
            };

            IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchStfld<VoidLightningOrb>(nameof(VoidLightningOrb.totalStrikes))
                );

            if (ilfound)
            {
                c.EmitDelegate<Func<int, int>>(totalStrikes => totalStrikes - 1);
            }

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchStfld<VoidLightningOrb>(nameof(VoidLightningOrb.secondsPerStrike))
                );

            if (ilfound)
            {
                c.Next.Operand = 0.15f;
            }
        }
    }
}
