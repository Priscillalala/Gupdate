using System;
using BepInEx;
using RoR2.CharacterAI;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Rendering.PostProcessing;
using RoR2.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Events;

namespace Gupdate.Gameplay.Monsters
{
    public class VoidRaid : ModBehaviour
    {
        //private GameObject clearCellEncounter;

        public override (string, string)[] GetLang() => new[]
        {
            ("GS_OBJECTIVE_CLEARCELLENCOUNTER", "Clear the <style=cIsDamage>Void Cell</style>.\n{0} monster(s) remaining")
        };

        public void Awake()
        {
            On.RoR2.VoidRaidGauntletController.Start += VoidRaidGauntletController_Start;
            IL.RoR2.VoidRaidGauntletController.TryOpenGauntlet += VoidRaidGauntletController_TryOpenGauntlet;
        }

        private void VoidRaidGauntletController_Start(On.RoR2.VoidRaidGauntletController.orig_Start orig, VoidRaidGauntletController self)
        {
            

            //self.phaseEncounters[1].spawns = Array.Empty<ScriptedCombatEncounter.SpawnInfo>();
            //self.phaseEncounters[1].gameObject.AddComponent<ClearCellEncounter>();
            //self.phaseEncounters[1].gameObject.AddComponent<ClearCellController>().enabled = false;

            //self.phaseEncounters[2].spawns = Array.Empty<ScriptedCombatEncounter.SpawnInfo>();
            //self.phaseEncounters[2].gameObject.AddComponent<ClearCellEncounter>();
            //self.phaseEncounters[2].gameObject.AddComponent<ClearCellController>().enabled = false;
            ScriptedCombatEncounter encounter = self.phaseEncounters[1];
            encounter.spawns = Array.Empty<ScriptedCombatEncounter.SpawnInfo>();
            if (encounter.TryGetComponent(out BossGroup bossGroup))
            {
                bossGroup.combatSquad.onMemberDiscovered -= bossGroup.OnMemberDiscovered;
                bossGroup.combatSquad.onMemberLost -= bossGroup.OnMemberLost;
                if (NetworkServer.active)
                {
                    bossGroup.combatSquad.onDefeatedServer -= bossGroup.OnDefeatedServer;
                    bossGroup.combatSquad.onMemberAddedServer -= bossGroup.OnMemberAddedServer;
                    bossGroup.combatSquad.onMemberDefeatedServer -= bossGroup.OnMemberDefeatedServer;
                }
                Destroy(bossGroup);
            }
            encounter.gameObject.AddComponent<ClearCellController>();
            encounter.GetComponent<CombatSquad>().grantBonusHealthInMultiplayer = false;

            if (self.phaseEncounters.Length < 4)
            {
                Array.Resize(ref self.phaseEncounters, 4);
            }
            self.phaseEncounters[3] = self.phaseEncounters[2];
            self.phaseEncounters[2] = self.phaseEncounters[1];


            orig(self);
        }

        private void VoidRaidGauntletController_TryOpenGauntlet(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (ilfound = c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject))))
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Action<VoidRaidGauntletController>>(gauntletController => 
                {
                    LogWarning($"Opening gauntlet! current index: {gauntletController.gauntletIndex}");
                    if (gauntletController.phaseEncounters[gauntletController.gauntletIndex].TryGetComponent(out ClearCellController clearCellController))
                    {
                        LogInfo("Adding monster credit!");
                        CombatDirector combatDirector = gauntletController.currentDonut.combatDirector;
                        combatDirector.monsterCredit += (int)(600f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
                        combatDirector.combatSquad = clearCellController.combatSquad;
                        combatDirector.SpendAllCreditsOnMapSpawns(gauntletController.currentDonut.returnPoint);
                        LogInfo("enabling cell clear!");
                        //clearCellController.enabled = true;
                    }
                });
            }
        }

        /*[RequireComponent(typeof(ScriptedCombatEncounter))]
        public class ClearCellEncounter : MonoBehaviour
        {
            public void Awake()
            {
                base.GetComponent<ScriptedCombatEncounter>().onBeginEncounter += _ => base.GetComponent<ClearCellController>().enabled = true;
            }
        }*/

        [RequireComponent(typeof(CombatSquad))]
        public class ClearCellController : MonoBehaviour
        {
            public bool shouldDisplayObjective
            {
                get => displayingObjective;
                set
                {
                    if (displayingObjective != value)
                    {
                        if (value)
                        {
                            ObjectivePanelController.collectObjectiveSources += AddDisplayObjective;
                        }
                        else
                        {
                            ObjectivePanelController.collectObjectiveSources -= AddDisplayObjective;
                        }
                        displayingObjective = value;
                    }
                }
            }

            private void AddDisplayObjective(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
            {
                objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = master,
                    objectiveType = typeof(ClearCellObjectiveTracker),
                    source = this
                });
            }

            private bool displayingObjective;
            public CombatSquad combatSquad;
            //private float clearCheckTimer;
            private bool hasEnabledIndicators;
            private readonly HashSet<NetworkInstanceId> indicatedNetIds = new HashSet<NetworkInstanceId>();
            //public const float clearCheckFrequency = 0.5f;

            /*public void FixedUpdate()
            {
                clearCheckTimer += Time.fixedDeltaTime;
                if (clearCheckTimer >= clearCheckFrequency)
                {
                    clearCheckTimer -= clearCheckFrequency;
                    ReadOnlyCollection<TeamComponent> monsters = TeamComponent.GetTeamMembers(TeamIndex.Monster);
                    int count = monsters.Count;
                    if (count <= 0)
                    {
                        Debug.Log("Finished cell encounter!");
                        shouldDisplayObjective = false;
                        base.enabled = false;
                        if (VoidRaidGauntletController.instance)
                        {
                            VoidRaidGauntletController.instance.TryOpenGauntlet(VoidRaidGauntletController.instance.currentDonut.crabPosition.position, NetworkInstanceId.Invalid);
                        }
                        return;
                    }
                    if (hasEnabledIndicators || count <= 3)
                    {
                        hasEnabledIndicators = true;
                        foreach (TeamComponent teamComponent in monsters)
                        {
                            if (teamComponent && teamComponent.body && teamComponent.body.master)
                            {
                                RequestIndicatorForMaster(teamComponent.body.master);
                            }
                        }
                    }
                }
            }*/

            public void Awake()
            {
                combatSquad = base.GetComponent<CombatSquad>();
                combatSquad.onDefeatedServer += OnDefeatedServer;
                combatSquad.onDefeatedServerLogicEvent ??= new UnityEvent();
                combatSquad.onDefeatedServerLogicEvent.AddListener(OnDefeatedAny);
                combatSquad.onDefeatedClientLogicEvent ??= new UnityEvent();
                combatSquad.onDefeatedClientLogicEvent.AddListener(OnDefeatedAny);
            }

            private void OnDefeatedServer()
            {
                Debug.Log("Finished cell encounter!");
                ResetCombatSquad();
                if (VoidRaidGauntletController.instance)
                {
                    VoidRaidGauntletController.instance.TryOpenGauntlet(VoidRaidGauntletController.instance.currentDonut.crabPosition.position, NetworkInstanceId.Invalid);
                }
            }

            private void OnDefeatedAny()
            {
                shouldDisplayObjective = false;
                ResetCombatSquad();
            }

            public void ResetCombatSquad()
            {
                combatSquad.defeatedServer = false;
                combatSquad.memberHistory.Clear();
                combatSquad.membersList.Clear();
            }

            public void Update()
            {
                //hasEnabledIndicators |= combatSquad.memberCount <= 3;
                if (combatSquad.memberCount <= 3)
                {
                    foreach (CharacterMaster master in combatSquad.readOnlyMembersList)
                    {
                        RequestIndicatorForMaster(master);
                    }
                }
            }

            public void RequestIndicatorForMaster(CharacterMaster master)
            {
                if (!indicatedNetIds.Contains(master.netId))
                {
                    GameObject bodyObject = master.GetBodyObject();
                    if (bodyObject)
                    {
                        TeamComponent component = bodyObject.GetComponent<TeamComponent>();
                        if (component)
                        {
                            indicatedNetIds.Add(master.netId);
                            component.RequestDefaultIndicator(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/InfiniteTowerAssets/WaveEndPositionIndicator.prefab").WaitForCompletion());
                        }
                    }
                }
            }

            public void OnEnable()
            {
                On.RoR2.VoidRaidGauntletController.OnAuthorityPlayerExit += VoidRaidGauntletController_OnAuthorityPlayerExit;
            }

            private void VoidRaidGauntletController_OnAuthorityPlayerExit(On.RoR2.VoidRaidGauntletController.orig_OnAuthorityPlayerExit orig, VoidRaidGauntletController self)
            {
                orig(self);
                shouldDisplayObjective = true;
            }

            public void OnDisable()
            {
                On.RoR2.VoidRaidGauntletController.OnAuthorityPlayerExit -= VoidRaidGauntletController_OnAuthorityPlayerExit;
            }

            public class ClearCellObjectiveTracker : ObjectivePanelController.ObjectiveTracker
            {
                public override string GenerateString()
                {
                    return string.Format(Language.GetString("GS_OBJECTIVE_CLEARCELLENCOUNTER"), (sourceDescriptor.source as ClearCellController)?.combatSquad.memberCount ?? 0);
                }

                public override bool IsDirty() => true;
            }
        }
    }
}
