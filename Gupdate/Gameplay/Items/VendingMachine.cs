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

namespace Gupdate.Gameplay.Items
{
    public class VendingMachine : ModBehaviour
    {
        private static GameObject fakeActor;

        public void Awake()
        {
            fakeActor = new GameObject("Fake Actor").InstantiateClone("Fake Actor", false);
            BoxCollider boxCollider = fakeActor.AddComponent<BoxCollider>();
            boxCollider.isTrigger = false;
            boxCollider.center = new Vector3(3.791681e-06f, 0.02f, 1.66f);
            boxCollider.size = new Vector3(2.300786f, 1.47f, 4.24f);
            fakeActor.layer = 8;

            Addressables.LoadAssetAsync<GameObject>("e69e4c37270ee1f4a8ecd2e60c03faad").Completed += handle =>
            {
                handle.Result.AddComponent<AddFakeActor>();
            };
        }

        public class AddFakeActor : MonoBehaviour
        {
            public void Awake()
            {
                Instantiate(fakeActor, base.transform.Find("mdlVendingMachine")).transform.localPosition = Vector3.zero;
                Destroy(this);
            }
        }
    }
}
