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
    public class HuntersHarpoon : ModBehaviour
    {
        /*public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_TOOTH_DESC", "Killing an enemy spawns a <style=cIsHealing>healing orb</style> that heals for <style=cIsHealing>4</style> plus an additional <style=cIsHealing>2% <style=cStack>(+2% per stack)</style></style> of <style=cIsHealing>maximum health</style>."),
            ("ITEM_MEDKIT_DESC", "2 seconds after getting hurt, <style=cIsHealing>heal</style> for <style=cIsHealing>10</style> plus an additional <style=cIsHealing>5% <style=cStack>(+5% per stack)</style></style> of <style=cIsHealing>maximum health</style>."),
        };*/

        public void Awake()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
        }

        private void GlobalEventManager_OnCharacterDeath(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(DLC1Content), nameof(DLC1Content.Items.MoveSpeedOnKill), out int locStackIndex))
            {
                ilfound = c.TryGotoNext(MoveType.Before, 
                    x => x.MatchLdcI4(out _), 
                    x => x.MatchStloc(out _),
                    x => x.MatchLdcR4(out _),
                    x => x.MatchLdloc(out _),
                    x => x.MatchConvR4(),
                    x => x.MatchLdcR4(out _),
                    x => x.MatchMul(),
                    x => x.MatchAdd(),
                    x => x.MatchStloc(out _)
                    );

                if (ilfound)
                {
                    //c.Next.OpCode = OpCodes.Ldc_I4;
                    //c.Next.Operand = 4;
                    c.Index += 2;
                    c.Next.Operand = 2f;
                    c.Index += 3;
                    c.Next.Operand = 1f;
                }
            }
        }
    }
}
