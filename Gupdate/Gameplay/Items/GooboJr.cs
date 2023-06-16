using System;
using BepInEx;
using EntityStates.FlyingVermin.Weapon;
using EntityStates.Vermin.Weapon;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gupdate.Gameplay.Items
{
    public class GooboJr : ModBehaviour
    {
        public void Awake()
        {
            LanguageAPI.Add("EQUIPMENT_GUMMYCLONE_DESC", "Spawn a gummy clone with <style=cIsDamage>100% damage</style> and <style=cIsHealing>100% health</style> that <style=cIsUtility>inherits all your items</style>. Expires in <style=cIsUtility>30</style> seconds.");
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GummyClone/GummyCloneProjectile.prefab").Completed += handle =>
            {
                GummyCloneProjectile gummyCloneProjectile = handle.Result.GetComponent<GummyCloneProjectile>();
                gummyCloneProjectile.damageBoostCount = 0;
                gummyCloneProjectile.hpBoostCount = 0;
            };
            IL.RoR2.Projectile.GummyCloneProjectile.SpawnGummyClone += GummyCloneProjectile_SpawnGummyClone;
            On.RoR2.CharacterModel.UpdateMaterials += CharacterModel_UpdateMaterials;
        }

        private void CharacterModel_UpdateMaterials(On.RoR2.CharacterModel.orig_UpdateMaterials orig, CharacterModel self)
        {
            orig(self);
            if (self.IsGummyClone() && self.visibility == VisibilityLevel.Visible)
            {
                for (int i = 0; i < self.parentedPrefabDisplays.Count; i++)
                {
                    ItemDisplay itemDisplay = self.parentedPrefabDisplays[i].itemDisplay;
                    SetItemDisplayGummy(itemDisplay);
                }
            }
        }
        public void SetItemDisplayGummy(ItemDisplay itemDisplay)
        {
            for (int i = 0; i < itemDisplay.rendererInfos.Length; i++)
            {
                CharacterModel.RendererInfo rendererInfo = itemDisplay.rendererInfos[i];
                if (!rendererInfo.ignoreOverlays) 
                {
                    rendererInfo.renderer.material = CharacterModel.gummyCloneMaterial;
                }
            }
        }

        private void GummyCloneProjectile_SpawnGummyClone(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locDirectorSpawnRequestIndex = -1;
            /*bool found = c.TryGotoNext(MoveType.After,
                x => x.MatchNewobj<DirectorSpawnRequest>(),
                x => x.MatchStloc(out locDirectorSpawnRequestIndex)
                );*/
            bool found = c.TryGotoNext(MoveType.Before,
                x => x.MatchCall<DirectorCore>("get_instance"),
                x => x.MatchLdloc(out locDirectorSpawnRequestIndex),
                x => x.MatchCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject))
                );
            if (found)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, locDirectorSpawnRequestIndex);
                c.EmitDelegate<Action<GummyCloneProjectile, DirectorSpawnRequest>>((gummyCloneProjectile, spawnRequest) =>
                {
                    if (spawnRequest.spawnCard is MasterCopySpawnCard mcsc)
                    {
                        if (mcsc.srcItemStacks != null)
                        {
                            ItemCatalog.ReturnItemStackArray(mcsc.srcItemStacks);
                            mcsc.srcItemStacks = null;
                            /*mcsc.itemsToGrant = new ItemCountPair[]
                            {
                                new ItemCountPair{itemDef = DLC1Content.Items.GummyCloneIdentifier, count = 1 },
                                new ItemCountPair{itemDef = RoR2Content.Items.BoostDamage, count = gummyCloneProjectile.damageBoostCount },
                                new ItemCountPair{itemDef = RoR2Content.Items.BoostHp, count = gummyCloneProjectile.hpBoostCount },
                            };*/
                        }
                    }
                    Debug.Log("emit delegate");
                    spawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(spawnRequest.onSpawnedServer, new Action<SpawnCard.SpawnResult>(OnGummySpawnedServer));
                });
            }
        }
        public static void OnGummySpawnedServer(SpawnCard.SpawnResult spawnResult)
        {
            Debug.Log("spawned server!");

            CharacterMaster characterMaster = spawnResult.spawnedInstance?.GetComponent<CharacterMaster>();
            if (characterMaster)
            {
                if (spawnResult.spawnRequest.spawnCard is MasterCopySpawnCard mcsc && mcsc.srcCharacterMaster)
                {
                    characterMaster.inventory.AddItemsFrom(mcsc.srcCharacterMaster.inventory);

                }
                Debug.Log("alter inventory");
                //if (ownerBody.inventory) characterMaster.inventory.CopyItemsFrom(ownerBody.inventory);
                characterMaster.inventory.GiveItem(DLC1Content.Items.GummyCloneIdentifier, 1);
                //characterMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, 10);
                //characterMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, 10);
                characterMaster.inventory.ResetItem(RoR2Content.Items.UseAmbientLevel);
            }
            /*if (gummyCloneProjectile.TryGetComponent(out ProjectileController projectileController) && projectileController.owner && projectileController.owner.TryGetComponent(out CharacterBody ownerBody))
            {
                GSUtil.Log("owner body");
                
            }*/
        }
    }
}
