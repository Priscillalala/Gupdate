using System;
using BepInEx;
using UnityEngine;
using Gupdate.Gameplay;
using Gupdate.Gameplay.Items;
using Gupdate.Gameplay.Monsters;
using Gupdate.QOL;
using BepInEx.Logging;
using System.Security.Permissions;
using System.Security;
using R2API;
using System.Collections.Generic;
using System.Linq;
using System.IO;

#pragma warning disable
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
#pragma warning restore

namespace Gupdate
{
    [BepInPlugin("com.groovesalad.Gupdate", "Gupdate", "1.0.0")]
    public class Gupdate : BaseUnityPlugin
    {
        public static Gupdate Instance { get; private set; }
        public static ManualLogSource Loggup { get; private set; }
        public static AssetBundle assets { get; private set; }

        public void Awake()
        {
            Instance = this;
            Loggup = Logger;
            assets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "gupdassets"));

            GameObject gupdate = new GameObject(nameof(Gupdate),
                typeof(BenthicBloom),
                typeof(BisonSteak),
                typeof(DelicateWatch),
                typeof(Egocentrism),
                typeof(ExecutiveCard),
                typeof(GooboJr),
                typeof(ICBM),
                typeof(LaserScope),
                typeof(Opal),
                typeof(PlasmaShrimp),
                typeof(Preon),
                typeof(SaferSpaces),
                typeof(Wungus),
                typeof(Mithrix),
                typeof(Vermin),
                typeof(Acrid),
                typeof(Captain),
                typeof(Commando),
                typeof(Huntress),
                typeof(Merc),
                typeof(LowHealthFraction),
                typeof(VoidCamp),
                typeof(ConsumedKeys),
                typeof(ConsumedRegenScrap),
                typeof(LogbookBossOrdering),
                typeof(SDPDisplay),
                typeof(TricornMaterial),
                typeof(VoidAlliesInLogbook),
                typeof(VoidBarnacleMaterial)
                );

            ModBehaviour[] modBehaviours = gupdate.GetComponents<ModBehaviour>();

            LanguageAPI.Add(modBehaviours.SelectMany(x => x.GetLang()).ToDictionary(x => x.Item1, x => x.Item2));

            DontDestroyOnLoad(gupdate);
        }
    }
}
