using SALT.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SALT.Utils
{
    public static class GameObjectUtils
    {
        public static object GameContext { get; internal set; }

        public static string PrintObjectTree(GameObject obj)
        {
            while ((UnityEngine.Object)obj.transform.parent != (UnityEngine.Object)null)
                obj = obj.transform.parent.gameObject;
            string indent = "";
            StringBuilder builder = new StringBuilder();
            GameObjectUtils.PrintObjectTreeInternal(obj, indent, builder);
            return builder.ToString();
        }

        private static void PrintObjectTreeInternal(
          GameObject obj,
          string indent,
          StringBuilder builder)
        {
            builder.AppendLine(indent + "GameObject: " + obj.name);
            indent += "    ";
            builder.AppendLine(indent + "components:");
            string indent1 = indent + "    ";
            List<Component> list = obj.GetComponents().ToList();
            StringComparer c = StringComparer.Create(CultureInfo.CurrentCulture, true);
            list.Sort((Comparison<Component>)((component, component1) => c.Compare(component.GetType().Name, component1.GetType().Name)));
            foreach (Component component in list)
            {
                MeshRenderer meshRenderer = component as MeshRenderer;
                SkinnedMeshRenderer skinnedMeshRenderer = component as SkinnedMeshRenderer;
                Transform transform = component as Transform;
                if ((bool)(UnityEngine.Object)transform)
                {
                    builder.AppendLine(indent1 + component.GetType().Name + " " + (object)transform.localPosition + ", " + (object)transform.localRotation + " " + (object)transform.localScale);
                }
                else
                {
                    builder.AppendLine(indent1 + component.GetType().Name);
                    if ((bool)(UnityEngine.Object)meshRenderer)
                    {
                        string str = indent1 + "    ";
                        builder.AppendLine(str + "Material: " + meshRenderer.material.name);
                    }
                    if ((bool)(UnityEngine.Object)skinnedMeshRenderer)
                    {
                        string str = indent1 + "    ";
                        builder.AppendLine(str + "Material: " + skinnedMeshRenderer.material.name);
                    }
                }
            }
            builder.AppendLine(indent + "children: ");
            for (int index = 0; index < obj.transform.childCount; ++index)
                GameObjectUtils.PrintObjectTreeInternal(obj.transform.GetChild(index).gameObject, indent1, builder);
        }

        public static void Prefabitize(GameObject obj)
        {
            obj.transform.SetParent(Main.prefabParent, false);
        }

        public static GameObject InstantiateInactive(GameObject original)
        {
            GameObject newObj = GameObject.Instantiate(original, Main.prefabParent, true);
            newObj.SetActive(false);
            newObj.transform.SetParent(null, false);
            return newObj;
        }



        public static GameObject InstantiateInactive(GameObject original, bool keepOriginalName = false)
        {
            GameObject newObj = InstantiateInactive(original);
            if (keepOriginalName) newObj.name = original.name;
            return newObj;
        }

        public static GameObject InstantiateInactive(GameObject original, Transform parent, bool keepOriginalName = false)
        {
            GameObject newObj = InstantiateInactive(original, keepOriginalName);
            newObj.transform.SetParent(parent);
            return newObj;
        }

        public static GameObject InstantiateInactive(GameObject original, Transform parent, bool worldPositionStays, bool keepOriginalName = false)
        {
            GameObject newObj = InstantiateInactive(original, keepOriginalName);
            newObj.transform.SetParent(parent, worldPositionStays);
            return newObj;
        }

        public static GameObject InstantiateInactive(GameObject original, Vector3 position, Quaternion rotation, bool keepOriginalName = false)
        {
            GameObject newObj = InstantiateInactive(original, keepOriginalName);
            newObj.transform.SetPositionAndRotation(position, rotation);
            return newObj;
        }

        public static GameObject InstantiateInactive(GameObject original, Vector3 position, Quaternion rotation, Transform parent, bool keepOriginalName = false)
        {
            GameObject newObj = InstantiateInactive(original, parent, keepOriginalName);
            newObj.transform.SetPositionAndRotation(position, rotation);
            return newObj;
        }

        public static GameObject InstantiateInactive(GameObject original, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays, bool keepOriginalName = false)
        {
            GameObject newObj = InstantiateInactive(original, parent, worldPositionStays, keepOriginalName);
            newObj.transform.SetPositionAndRotation(position, rotation);
            return newObj;
        }
    }
}
