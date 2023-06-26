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

namespace Gupdate.Gameplay.Items
{
    public class Raincoat : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_IMMUNETODEBUFF_PICKUP", "Prevent debuffs, instead gaining a temporary barrier. Recharges over time."),
            ("ITEM_IMMUNETODEBUFF_DESC", "Prevents an incoming <style=cIsDamage>debuff</style> and instead grants a <style=cIsHealing>temporary barrier</style> for <style=cIsHealing>100 health <style=cStack>(+100 per stack)</style></style>. Recharges every <style=cIsUtility>5</style> seconds</style>."),
        };

        public void Awake()
        {
            Addressables.LoadAssetAsync<BuffDef>("RoR2/DLC1/ImmuneToDebuff/bdImmuneToDebuffReady.asset").Completed += handle =>
            {
                handle.Result.canStack = false;
                handle.Result.iconSprite = Gupdate.assets.LoadAsset<Sprite>("texBuffImmuneToDebuffIcon");
            };

            IL.RoR2.Items.ImmuneToDebuffBehavior.FixedUpdate += ImmuneToDebuffBehavior_FixedUpdate;
            IL.RoR2.Items.ImmuneToDebuffBehavior.TryApplyOverride += ImmuneToDebuffBehavior_TryApplyOverride;
        }

        private void ImmuneToDebuffBehavior_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<BaseItemBodyBehavior>(nameof(BaseItemBodyBehavior.stack))
                );

            if (ilfound)
            {
                c.EmitDelegate<Func<int, int>>(stack => Mathf.Min(stack, 1));
            }
        }

        private void ImmuneToDebuffBehavior_TryApplyOverride(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int locBehaviourIndex = -1;
            ilfound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out locBehaviourIndex),
                x => x.MatchLdfld<ImmuneToDebuffBehavior>(nameof(ImmuneToDebuffBehavior.healthComponent))
                ) && c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<HealthComponent>(nameof(HealthComponent.AddBarrier))
                );

            if (ilfound)
            {
                c.Emit(OpCodes.Ldloc, locBehaviourIndex);
                c.EmitDelegate<Func<float, ImmuneToDebuffBehavior, float>>((barrier, behaviour) => behaviour.stack * 100f);
            }
        }
    }
}
