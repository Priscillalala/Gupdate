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
                typeof(Bands),
                typeof(BenthicBloom),
                typeof(BisonSteak),
                typeof(DelicateWatch),
                //typeof(Egocentrism),
                typeof(ExecutiveCard),
                typeof(GooboJr),
                typeof(HuntersHarpoon),
                typeof(ICBM),
                typeof(IgnitionTank),
                typeof(Knurl),
                typeof(LaserScope),
                typeof(LysateCell),
                typeof(MedkitAndTooth),
                typeof(Opal),
                typeof(PlasmaShrimp),
                typeof(Polylute),
                typeof(Preon),
                typeof(Raincoat),
                typeof(SaferSpaces),
                typeof(SDP),
                typeof(SingularityBand),
                typeof(Vase),
                typeof(VendingMachine),
                typeof(Wungus),
                typeof(Zoea),
                typeof(BeetleQueen),
                typeof(ElderLemurian),
                typeof(Gup),
                typeof(Mithrix),
                typeof(ReaverAndJailer),
                typeof(Vermin),
                typeof(XiConstruct),
                typeof(HiddenRealms),
                typeof(MeadowMeetsPeak),
                typeof(Moon),
                typeof(SiphonedForest),
                typeof(VoidLocus),
                typeof(Wetland),
                typeof(Acrid),
                typeof(Artificer),
                typeof(Bandit),
                typeof(Captain),
                typeof(Commando),
                typeof(Engi),
                typeof(Huntress),
                typeof(Merc),
                typeof(Railgunner),
                typeof(REX),
                typeof(VoidFiend),
                typeof(Burn),
                typeof(LowHealthFraction),
                typeof(VoidCamp),
                typeof(VoidCampStages),
                typeof(ConsumedKeys),
                typeof(ConsumedRegenScrap),
                typeof(DuplicateSeers),
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
