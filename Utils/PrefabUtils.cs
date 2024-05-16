using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SALT.Utils
{
    public static class PrefabUtils
    {
        public static void ReplaceFieldsWith<T>(GameObject prefab, T original, T newValue)
        {
            foreach (Component componentsInChild in prefab.GetComponentsInChildren<Component>(true))
            {
                if ((bool)(UnityEngine.Object)componentsInChild)
                {
                    foreach (FieldInfo field in componentsInChild.GetType().GetFields())
                    {
                        if (field.FieldType == typeof(T) && ((T)field.GetValue((object)componentsInChild)).Equals((object)original))
                            field.SetValue((object)componentsInChild, (object)newValue);
                    }
                }
            }
        }

        public static GameObject CopyPrefab(GameObject prefab)
        {
            return GameObject.Instantiate(prefab, Main.prefabParent, false);
        }

        public static UnityEngine.Object DeepCopyObject(UnityEngine.Object ob)
        {
            Dictionary<UnityEngine.Object, UnityEngine.Object> refToRef = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
            UnityEngine.Object DeepCopyObject_Internal(UnityEngine.Object obj)
            {
                if (!obj) return obj;
                if (refToRef.TryGetValue(obj, out var old)) return old;
                if (refToRef.Values.Contains(obj)) return obj;
                var newObj = UnityEngine.Object.Instantiate(obj);
                if (!typeof(ScriptableObject).IsAssignableFrom(newObj.GetType())) return newObj;
                refToRef[obj] = newObj;
                System.Object ProcessObject(System.Object theObj)
                {
                    if (theObj == null) return null;

                    if (typeof(Component).IsAssignableFrom(theObj.GetType()) || typeof(GameObject).IsAssignableFrom(theObj.GetType()))
                    {
                        return theObj;
                    }
                    else
                    if (typeof(UnityEngine.Object).IsAssignableFrom(theObj.GetType()))
                    {
                        return DeepCopyObject_Internal((UnityEngine.Object)theObj);
                    }
                    else if (theObj.GetType().IsArray)
                    {
                        var g = theObj as Array;
                        for (int i = 0; i < g.Length; i++)
                        {
                            g.SetValue(ProcessObject(g.GetValue(i)), i);
                        }

                    }
                    else if (!theObj.GetType().IsPrimitive && !typeof(string).IsAssignableFrom(theObj.GetType()) && !theObj.GetType().IsEnum)
                    {
                        foreach (var v in theObj.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
                        {
                            v.SetValue(theObj, ProcessObject(v.GetValue(theObj)));
                        }
                    }

                    return theObj;
                }

                foreach (var v in newObj.GetType().GetFields())
                {
                    v.SetValue(newObj, ProcessObject(v.GetValue(newObj)));
                }
                return newObj;
            }
            return DeepCopyObject_Internal(ob);
        }
    }
}
