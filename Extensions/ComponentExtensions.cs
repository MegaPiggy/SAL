using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;
using UnityEngine;

namespace SALT.Extensions
{
    public static class ComponentExtensions
    {
        private static readonly string[] SKIP_PROP_NAMES = new string[3]
        {
            "sleepAngularVelocity",
            "sleepVelocity",
            "useConeFriction"
        };

        public static void RemoveComponent<T>(this Component removeFrom) where T : Component
        {
            removeFrom.gameObject.RemoveComponent<T>();
        }

        public static T AddComponent<T>(this Component addTo) where T : Component
        {
            return addTo.gameObject.AddComponent<T>();
        }

        public static T GetCopyOf<T>(this Component copyInto, T copyFrom) where T : Component
        {
            Type type = copyInto.GetType();
            if (type != copyFrom.GetType())
                return null;
            for (; type != typeof(Component) && type != null; type = type.BaseType)
            {
                foreach (PropertyInfo property in type.GetInstanceProperties())
                {
                    if (!SKIP_PROP_NAMES.Contains(property.Name) && property.CanWrite && property.CanRead)
                    {
                        if (property.Name != "material")
                        {
                            try
                            {
                                property.SetValue(copyInto, property.GetValue(copyFrom, null), null);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("ZOMG! Cannot set property when copying component. " + property + " err: " + ex.ParseTrace());
                            }
                        }
                    }
                }
                foreach (FieldInfo field in type.GetInstanceFields())
                {
                    if (field.IsPublic || field.GetCustomAttributes(typeof(SerializeField), true).Length != 0)
                    {
                        if (field.FieldType.IsValueType)
                            field.SetValue(copyInto, field.GetValue(copyFrom));
                        else if (field.FieldType.IsSerializable)
                            field.SetValue(copyInto, field.GetValue(copyFrom).Clone());
                        else
                            field.SetValue(copyInto, field.GetValue(copyFrom));
                    }
                }
            }
            return copyInto as T;
        }

        public static T GetComponentInParent<T>(this Component component, bool includeInactive = false) where T : Component => component.gameObject.GetComponentInParent<T>(includeInactive);

        public static T GetRequiredComponent<T>(this Component component) where T : Component => component.gameObject.GetRequiredComponent<T>();

        public static T GetRequiredComponentInParent<T>(this Component component, bool includeInactive = false) where T : Component => component.gameObject.GetRequiredComponentInParent<T>(includeInactive);

        public static T GetRequiredComponentInChildren<T>(
          this Component component,
          bool includeInactive = false)
          where T : Component
        {
            return component.gameObject.GetRequiredComponentInChildren<T>(includeInactive);
        }

        /// <summary>
        /// Creates a copy of a material
        /// </summary>
        /// <param name="mat">The material to copy</param>
        /// <param name="copyProps">Should the properties get deep copied? Some might require a deep copy</param>
        public static Material Copy(this Material mat, bool copyProps = false)
        {
            Material copy = Object.Instantiate(mat);

            if (copyProps)
                copy.CopyPropertiesFromMaterial(mat);

            return copy;
        }

        // MATERIAL CONTROL
        public static Material SetInfo(this Material mat, Color color, string name)
        {
            mat.SetColor("_Color", color);
            mat.name = name;
            return mat;
        }

        /// <summary>
        /// Set's alpha channel for the Material `_Color` property
        /// </summary>
        /// <param name="material">Material to operate with.</param>
        /// <param name="value">Alpha channel value.</param>
        public static void SetAlpha(this Material material, float value)
        {
            if (material.HasProperty("_Color"))
            {
                var color = material.color;
                color.a = value;
                material.color = color;
            }
        }

        // OBJECT CONTROL
        public static T[] Group<T>(this T obj) where T : Object => new T[1]
        {
            obj
        };

        public static T[] Group<T>(this T obj, params T[] others) where T : Object
        {
            List<T> objList = new List<T>{ obj };
            objList.AddRange((IEnumerable<T>)others);
            return objList.ToArray();
        }

        // PRIVATE FIELDS STUFF
        [System.Obsolete("Use ObjectExtensions.SetField instead.")]
        public static T SetPrivateField<T>(this T comp, string name, object value) where T : Component
        {
            comp.SetField(name, value);

            return comp;
        }

        [System.Obsolete("Use ObjectExtensions.SetProperty instead.")]
        public static T SetPrivateProperty<T>(this T comp, string name, object value) where T : Component
        {
            comp.SetProperty(name, value);

            return comp;
        }

        [System.Obsolete("Use ObjectExtensions.GetField instead.")]
        public static E GetPrivateField<E>(this Component comp, string name) => comp.GetField<E>(name);

        [System.Obsolete("Use ObjectExtensions.GetProperty instead.")]
        public static E GetPrivateProperty<E>(this Component comp, string name) => comp.GetProperty<E>(name);

        public static void CopyAllTo<T>(this T comp, T otherComp) where T : Component
        {
            foreach (FieldInfo field in comp.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
            {
                try
                {
                    field.SetValue(otherComp, field.GetValue(comp));
                }
                catch
                {
                }
            }
            foreach (FieldInfo field in comp.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if ((uint)field.GetCustomAttributes(typeof(SerializeField), false).Length > 0U)
                {
                    try
                    {
                        field.SetValue(otherComp, field.GetValue(comp));
                    }
                    catch
                    {
                    }
                }
            }
            foreach (PropertyInfo property in comp.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
            {
                try
                {
                    property.SetValue(otherComp, property.GetValue(comp, (object[])null), (object[])null);
                }
                catch
                {
                }
            }
        }


        /// <summary>
        /// Find all Components of specified interface
        /// </summary>
        public static T[] FindObjectsOfInterface<T>() where T : class
        {
            var monoBehaviours = Object.FindObjectsOfType<Transform>();

            return monoBehaviours.Select(behaviour => behaviour.GetComponent(typeof(T))).OfType<T>().ToArray();
        }

        /// <summary>
        /// Find all Components of specified interface along with Component itself
        /// </summary>
        public static ComponentOfInterface<T>[] FindObjectsOfInterfaceAsComponents<T>() where T : class
        {
            return Object.FindObjectsOfType<Component>()
                .Where(c => c is T)
                .Select(c => new ComponentOfInterface<T>(c, c as T)).ToArray();
        }

        public struct ComponentOfInterface<T> : IEquatable<ComponentOfInterface<T>>
        {
            public readonly Component Component;
            public readonly T Interface;

            public ComponentOfInterface(Component component, T @interface)
            {
                Component = component;
                Interface = @interface;
            }

            public bool Equals(ComponentOfInterface<T> cof) => object.Equals(cof.Interface, this.Interface) && this.Component == cof.Component;
        }

        #region One Per Instance

        /// <summary>
        /// Get components with unique Instance ID
        /// </summary>
        public static T[] OnePerInstance<T>(this T[] components) where T : Component
        {
            if (components == null || components.Length == 0) return null;
            return components.GroupBy(h => h.transform.GetInstanceID()).Select(g => g.First()).ToArray();
        }

        /// <summary>
        /// Get hits with unique owner Instance ID
        /// </summary>
        public static RaycastHit2D[] OneHitPerInstance(this RaycastHit2D[] hits)
        {
            if (hits == null || hits.Length == 0) return null;
            return hits.GroupBy(h => h.transform.GetInstanceID()).Select(g => g.First()).ToArray();
        }

        /// <summary>
        /// Get colliders with unique owner Instance ID
        /// </summary>
        public static Collider2D[] OneHitPerInstance(this Collider2D[] hits)
        {
            if (hits == null || hits.Length == 0) return null;
            return hits.GroupBy(h => h.transform.GetInstanceID()).Select(g => g.First()).ToArray();
        }

        /// <summary>
        /// Get colliders with unique owner Instance ID
        /// </summary>
        public static List<Collider2D> OneHitPerInstanceList(this Collider2D[] hits)
        {
            if (hits == null || hits.Length == 0) return null;
            return hits.GroupBy(h => h.transform.GetInstanceID()).Select(g => g.First()).ToList();
        }

        #endregion

        public static Component GetParent(this Component target)
        {
            if ((UnityEngine.Object)target == (UnityEngine.Object)null)
                return (Component)null;
            return (UnityEngine.Object)target != (UnityEngine.Object)target.transform ? (Component)target.transform : (Component)target.transform.parent;
        }

        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            var toGet = component.gameObject.GetComponent<T>();
            if (toGet != null) return toGet;
            return component.gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Is the component present in the object?
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <param name="comp">The component if found, null if not</param>
        /// <typeparam name="T">The type of component</typeparam>
        /// <returns>True if the component is found, false otherwise</returns>
        public static bool HasComponent<T>(this Component obj, out T comp) where T : Component
        {
            comp = obj.GetComponent<T>();

            return comp != null;
        }

        /// <summary>
        /// Is the component present in the object?
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <typeparam name="T">The type of component</typeparam>
        /// <returns>True if the component is found, false otherwise</returns>
        public static bool HasComponent<T>(this Component obj) where T : Component
        {
            return obj.GetComponent<T>() != null;
        }
    }
}