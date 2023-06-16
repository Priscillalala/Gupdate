using System;
using BepInEx;
using R2API;
using RoR2;
using RoR2.UI.LogBook;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using RoR2.ExpansionManagement;
using System.Linq;

namespace Gupdate.QOL
{
    public class LogbookBossOrdering : ModBehaviour
    {
        public void Awake()
        {
            On.RoR2.UI.LogBook.LogBookController.BuildMonsterEntries += LogBookController_BuildMonsterEntries;
        }

        private Entry[] LogBookController_BuildMonsterEntries(On.RoR2.UI.LogBook.LogBookController.orig_BuildMonsterEntries orig, Dictionary<ExpansionDef, bool> expansionAvailability)
        {
            Entry[] entries = orig(expansionAvailability);
            return entries.OrderBy(x => x.extraData is CharacterBody body && body.isChampion).ToArray();
        }
    }
}
