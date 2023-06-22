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
    public class MedkitAndTooth : ModBehaviour
    {
        public override (string, string)[] GetLang() => new[]
        {
            ("ITEM_TOOTH_DESC", "Killing an enemy spawns a <style=cIsHealing>healing orb</style> that heals for <style=cIsHealing>4</style> plus an additional <style=cIsHealing>2% <style=cStack>(+2% per stack)</style></style> of <style=cIsHealing>maximum health</style>."),
            ("ITEM_MEDKIT_DESC", "2 seconds after getting hurt, <style=cIsHealing>heal</style> for <style=cIsHealing>10</style> plus an additional <style=cIsHealing>5% <style=cStack>(+5% per stack)</style></style> of <style=cIsHealing>maximum health</style>."),
        };

        public void Awake()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            IL.RoR2.CharacterBody.RemoveBuff_BuffIndex += CharacterBody_RemoveBuff_BuffIndex;
        }

        private void GlobalEventManager_OnCharacterDeath(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(RoR2Content), nameof(RoR2Content.Items.Tooth), out int locStackIndex))
            {
                ilfound = c.TryGotoNext(MoveType.Before, x => x.MatchLdcR4(out _), x => x.MatchStfld<HealthPickup>(nameof(HealthPickup.flatHealing)));

                if (ilfound)
                {
                    c.Next.Operand = 4f;
                }
            }
        }

        private void CharacterBody_RemoveBuff_BuffIndex(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (ilfound = Gutil.TryGotoStackLocIndex(c, typeof(RoR2Content), nameof(RoR2Content.Items.Medkit), out int locStackIndex))
            {
                ilfound = c.TryGotoNext(MoveType.Before, x => x.MatchLdcR4(out _), x => x.MatchStloc(out _));

                if (ilfound)
                {
                    c.Next.Operand = 10f;
                }
            }
        }
    }
}
