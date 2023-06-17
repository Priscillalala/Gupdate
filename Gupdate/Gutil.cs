using System;
using System.Collections.Generic;
using BepInEx;
using HG.GeneralSerializer;
using MonoMod.Cil;
using RoR2;
using UnityEngine;

namespace Gupdate
{
    public static class Gutil
    {
        public static bool TryFind(this Transform transform, string n, out Transform child)
        {
            return child = transform.Find(n);
        }
        public static IEnumerable<Transform> AllChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                yield return transform.GetChild(i);
            }
        }
        public static bool HasItem(this CharacterBody characterBody, ItemDef itemDef, out int stack) => HasItem(characterBody, itemDef ? itemDef.itemIndex : ItemIndex.None, out stack);
        public static bool HasItem(this CharacterBody characterBody, ItemIndex itemIndex, out int stack)
        {
            if (characterBody && characterBody.inventory)
            {
                stack = characterBody.inventory.GetItemCount(itemIndex);
                return stack > 0;
            }
            stack = 0;
            return false;
        }
        public static bool HasItem(this CharacterMaster characterMaster, ItemDef itemDef, out int stack) => HasItem(characterMaster, itemDef ? itemDef.itemIndex : ItemIndex.None, out stack);
        public static bool HasItem(this CharacterMaster characterMaster, ItemIndex itemIndex, out int stack)
        {
            if (characterMaster && characterMaster.inventory)
            {
                stack = characterMaster.inventory.GetItemCount(itemIndex);
                return stack > 0;
            }
            stack = 0;
            return false;
        }
        public static bool HasItem(this CharacterBody characterBody, ItemDef itemDef) => HasItem(characterBody, itemDef.itemIndex);
        public static bool HasItem(this CharacterBody characterBody, ItemIndex itemIndex) => characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(itemIndex) > 0;
        public static bool HasItem(this CharacterMaster characterMaster, ItemDef itemDef) => HasItem(characterMaster, itemDef.itemIndex);
        public static bool HasItem(this CharacterMaster characterMaster, ItemIndex itemIndex) => characterMaster && characterMaster.inventory && characterMaster.inventory.GetItemCount(itemIndex) > 0;
        public static void ClearDotStacksForType(this DotController dotController, DotController.DotIndex dotIndex)
        {
            for (int i = dotController.dotStackList.Count - 1; i >= 0; i--)
            {
                if (dotController.dotStackList[i].dotIndex == dotIndex)
                {
                    dotController.RemoveDotStackAtServer(i);
                }
            }
        }
        public static bool TryModifyFieldValue<T>(this EntityStateConfiguration entityStateConfiguration, string fieldName, T value)
        {
            ref SerializedField serializedField = ref entityStateConfiguration.serializedFieldsCollection.GetOrCreateField(fieldName);
            Type type = typeof(T);
            if (serializedField.fieldValue.objectValue && typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                serializedField.fieldValue.objectValue = value as UnityEngine.Object;
                return true;
            }
            else if (serializedField.fieldValue.stringValue != null && StringSerializer.CanSerializeType(type))
            {
                serializedField.fieldValue.stringValue = StringSerializer.Serialize(type, value);
                return true;
            }
            return false;
        }
        public static float StackScaling(float baseValue, float stackValue, int stack)
        {
            if (stack > 0)
            {
                return baseValue + ((stack - 1) * stackValue);
            }
            return 0f;
        }
        public static int StackScaling(int baseValue, int stackValue, int stack)
        {
            if (stack > 0)
            {
                return baseValue + ((stack - 1) * stackValue);
            }
            return 0;
        }
        public static bool HealthComponent_TakeDamage_TryFindLocDamageIndex(ILCursor iLCursor, out int locDamageIndex)
        {
            int i = -1;
            bool found = iLCursor.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damage)),
                x => x.MatchStloc(out i)
                );
            locDamageIndex = i;
            iLCursor.Index = 0;
            return found;
        }
        public static bool TryGotoStackLocIndex(ILCursor c, Type content, string itemName, out int locStackIndex)
        {
            int i = -1;
            bool found = c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(content.GetNestedType("Items").GetField(itemName)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(out i)
                );
            locStackIndex = i;
            return found;
        }
    }
}
