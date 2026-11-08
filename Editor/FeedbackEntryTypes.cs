using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Astra.Pulse.Editor
{
    /// <summary>
    /// Entry types an inspector can create in a <c>[SerializeReference]</c> slot: concrete <c>[Serializable]</c>
    /// classes implementing <see cref="IFeedbackEntry"/> with a parameterless constructor, from any assembly.
    /// </summary>
    public static class FeedbackEntryTypes
    {
        private const BindingFlags ConstructorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static IReadOnlyList<Type> GetAuthorable()
        {
            List<Type> result = new List<Type>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<IFeedbackEntry>())
            {
                if (IsAuthorable(type))
                    result.Add(type);
            }

            result.Sort((Type a, Type b) =>
            {
                int byName = string.Compare(GetDisplayName(a), GetDisplayName(b), StringComparison.OrdinalIgnoreCase);
                return byName != 0 ? byName : string.Compare(a.FullName, b.FullName, StringComparison.Ordinal);
            });
            return result;
        }

        public static bool IsAuthorable(Type type)
        {
            if (type == null || !type.IsClass || type.IsAbstract || type.ContainsGenericParameters)
                return false;
            if (!type.IsSerializable || typeof(UnityEngine.Object).IsAssignableFrom(type))
                return false;

            return type.GetConstructor(ConstructorFlags, null, Type.EmptyTypes, null) != null;
        }

        public static string GetDisplayName(Type type)
        {
            return type == null ? "Empty" : ObjectNames.NicifyVariableName(type.Name);
        }
    }
}
