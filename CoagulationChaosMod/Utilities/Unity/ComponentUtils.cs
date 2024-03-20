using Il2CppInterop.Runtime;
using UnityEngine;

namespace CoagulationChaosMod.Utilities.Unity
{
    internal static class ComponentUtils
    {
        internal static Component CopyComponent(Component original, GameObject destination)
        {
            Type type = original.GetType();
            Component copy = destination.AddComponent(Il2CppType.From(type));
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }
    }
}
