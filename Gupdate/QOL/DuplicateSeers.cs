using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;

namespace Gupdate.QOL
{
    public class DuplicateSeers : ModBehaviour
    {
        public void Awake()
        {
            IL.RoR2.BazaarController.SetUpSeerStations += BazaarController_SetUpSeerStations;
        }

        private void BazaarController_SetUpSeerStations(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int locListIndex = -1;
            ilfound = c.TryGotoNext(MoveType.After, x => x.MatchNewobj<List<SceneDef>>(), x => x.MatchStloc(out locListIndex))
                && c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(Util), nameof(Util.ShuffleList)));

            if (ilfound)
            {
                c.Emit(OpCodes.Ldloc, locListIndex);
                c.EmitDelegate<Action<List<SceneDef>>>(list =>
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        for (int j = list.Count - 1; j > i; j--)
                        {
                            if (list[i].baseSceneName == list[j].baseSceneName)
                            {
                                list.RemoveAt(j);
                            }
                        }
                    }
                });
            }
        }
    }
}
