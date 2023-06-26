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
using RoR2.Orbs;
using UnityEngine.Events;

namespace Gupdate.Gameplay.Items
{
    public class Vase : ModBehaviour
    {
        private static DeployableSlot gatewayDeployable;
        public override (string, string)[] GetLang() => new[]
        {
			("EQUIPMENT_GATEWAY_PICKUP", "Create a quantum tunnel between two locations. Can place up to 2."),
            ("EQUIPMENT_GATEWAY_DESC", "Create a <style=cIsUtility>quantum tunnel</style> of up to <style=cIsUtility>1000m</style> in length. Can place up to <style=cIsUtility>2</style>."),
        };

        public void Awake()
        {
            gatewayDeployable = DeployableAPI.RegisterDeployableSlot((master, multiplier) => 3);

            On.RoR2.EquipmentSlot.FireGateway += EquipmentSlot_FireGateway;
        }

        private bool EquipmentSlot_FireGateway(On.RoR2.EquipmentSlot.orig_FireGateway orig, EquipmentSlot self)
        {
			Ray aimRay = self.GetAimRay();
			float offset = 2f;
			float minDistance = offset * 2f;
			float maxDistance = 1000f;
			/*Rigidbody component = base.GetComponent<Rigidbody>();
			if (!component)
			{
				return false;
			}*/
			Vector3 position = aimRay.GetPoint(offset);
			if (Physics.Raycast(aimRay, out RaycastHit raycastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
			{
				Vector3 endPosition = raycastHit.point + raycastHit.normal * offset;
				//Vector3 direction = endPosition - position;
				//Vector3 directionNormalized = direction.normalized;
				Vector3 pointBPosition = endPosition;

				if (raycastHit.distance < minDistance)
                {
					return false;
                }
				/*RaycastHit raycastHit2;
				if (component.SweepTest(directionNormalized, out raycastHit2, direction.magnitude))
				{
					if (raycastHit2.distance < num2)
					{
						return false;
					}
					pointBPosition = position + directionNormalized * raycastHit2.distance;
				}*/
				GameObject instance = Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Zipline"));
				ZiplineController zipLineController = instance.GetComponent<ZiplineController>();
				zipLineController.SetPointAPosition(position);// + directionNormalized * offset);
				zipLineController.SetPointBPosition(pointBPosition);
				EventFunctions eventFunctions = instance.AddComponent<EventFunctions>();
				Deployable deployable = instance.AddComponent<Deployable>();
				deployable.onUndeploy = new UnityEvent();
				deployable.onUndeploy.AddListener(eventFunctions.DestroySelf);
				self.characterBody.master.AddDeployable(deployable, gatewayDeployable);
				NetworkServer.Spawn(instance);
				return true;
			}
			return false;
		}
    }
}
