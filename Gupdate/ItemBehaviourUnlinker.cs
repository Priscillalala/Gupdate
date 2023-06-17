using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Items;
using System;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using System.Linq;
using Mono.Cecil;

namespace Gupdate
{
    public static class ItemBehaviourUnlinker
    {
        private static HashSet<Type> unlinkedBaseItemBodyBehaviours = new HashSet<Type>();
        private static HashSet<string> unlinkedAddItemBehavioursNames = new HashSet<string>();

        [SystemInitializer(typeof(BaseItemBodyBehavior))]
        public static void Init()
        {
            if (unlinkedBaseItemBodyBehaviours.Count > 0)
            {
                HandleNetworkContext(BaseItemBodyBehavior.server);
                HandleNetworkContext(BaseItemBodyBehavior.client);
                HandleNetworkContext(BaseItemBodyBehavior.shared);
            }
            unlinkedBaseItemBodyBehaviours = null;

            if (unlinkedAddItemBehavioursNames.Count > 0)
            {
                IL.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            }
            else
            {
                unlinkedAddItemBehavioursNames = null;
            }
        }

        private static void CharacterBody_OnInventoryChanged(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int? previousIndex = null;
            while (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.AddItemBehavior)), x => x.MatchPop()))
            {
                if (c.Previous.Previous.Operand is GenericInstanceMethod genericInstanceMethod)
                {
                    TypeReference typeReference = genericInstanceMethod.GenericArguments[0];
                    if (typeReference != null && unlinkedAddItemBehavioursNames.Contains(typeReference.FullName) && previousIndex != null)
                    {
                        int range = c.Index - (int)previousIndex;
                        c.Index = (int)previousIndex;
                        c.RemoveRange(range);
                        continue;
                    }
                }
                previousIndex = c.Index;
            }
        }

        private static void HandleNetworkContext(BaseItemBodyBehavior.NetworkContextSet context)
        {
            for (int i = context.itemTypePairs.Length - 1; i >= 0; i--)
            {
                Type behaviourType = context.itemTypePairs[i].behaviorType;
                if (unlinkedBaseItemBodyBehaviours.Contains(behaviourType))
                {
                    ArrayUtils.ArrayRemoveAtAndResize(ref context.itemTypePairs, i);
                }
            }
        }
        public static void Add<T>() => Add(typeof(T));
        public static void Add(Type behaviourType)
        {
            if (typeof(BaseItemBodyBehavior).IsAssignableFrom(behaviourType))
            {
                unlinkedBaseItemBodyBehaviours.Add(behaviourType);
            }
            else if (typeof(CharacterBody.ItemBehavior).IsAssignableFrom(behaviourType))
            {
                unlinkedAddItemBehavioursNames.Add(behaviourType.FullName.Replace('+', '/'));
            }
        }
    }
}
