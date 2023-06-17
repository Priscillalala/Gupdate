using System;
using BepInEx;
using EntityStates.FlyingVermin.Weapon;
using EntityStates.Vermin.Weapon;
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
    public class Egocentrism : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_LUNARSUN_DESC", "Every <style=cIsUtility>3</style> <style=cStack>(-50% per stack)</style> seconds, gain an <style=cIsDamage>orbiting bomb</style> that detonates on impact for <style=cIsDamage>320%</style> damage, up to a maximum of <style=cIsUtility>3 <style=cStack>(+1 per stack)</style> bombs</style>. Every <style=cIsUtility>60</style> seconds, a random item is <style=cIsUtility>converted</style> into this item."),
        };

        public void Awake()
        {
            IL.RoR2.LunarSunBehavior.FixedUpdate += LunarSunBehavior_FixedUpdate;
        }

        private void LunarSunBehavior_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchCallvirt<CharacterBody>("get_damage"),
                x => x.MatchLdcR4(out _),
                x => x.MatchMul(),
                x => x.MatchStfld<FireProjectileInfo>(nameof(FireProjectileInfo.damage))
            );
            if (ilfound)
            {
                c.Index++;
                c.Next.Operand = 3.2f;
            }
            ILLabel skipToLabel = null;
            ilfound = c.TryGotoNext(MoveType.Before,
                x => x.MatchCallvirt<ItemDef>("get_tier"),
                x => x.MatchLdcI4((int)ItemTier.NoTier),
                x => x.MatchBeq(out skipToLabel)
            );
            if (ilfound)
            {
                c.RemoveRange(3);
                c.EmitDelegate<Func<ItemDef, bool>>((itemDef) => itemDef.hidden);
                c.Emit(OpCodes.Brtrue, skipToLabel);
            }
        }
    }
}
