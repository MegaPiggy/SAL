using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SALT.Utils;

namespace SALT.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets the full name of a game object (includes the parents)
        /// </summary>
        /// <param name="this">The object to get the name from</param>
        /// <returns>The full name of this game object</returns>
        public static string GetFullName(this GameObject @this)
        {
            string str = @this.name;
            for (Transform parent = @this.transform.parent; (Object)parent != (Object)null; parent = parent.parent)
                str = parent.name + "/" + str;
            return str;
        }

        public static string GetPath(this GameObject gameObject)
        {
            if (gameObject.GetComponent<Transform>() == null)
                throw new System.NullReferenceException("Cannot get path of GameObject because transform is null.");
            return gameObject.transform.GetHierarchyString();
        }
        public static T Initialize<T>(this T obj, System.Action<T> action) where T : UnityEngine.Object
        {
            action(obj);
            return obj;
        }

        public static void SetActiveRecursivelyExt(this GameObject obj, bool state)
        {
            obj.SetActive(state);
            foreach (Transform child in obj.transform)
            {
                SetActiveRecursivelyExt(child.gameObject, state);
            }
        }

        public static void SetChildActive(this GameObject obj,string name, bool state)
        {
            if (obj.name.ToLower() == name.ToLower())
                obj.SetActive(state);
            else
                obj.FindChild(name, true).SetActive(state);
        }

        public static T GetComponentInParent<T>(this GameObject gameObject, bool includeInactive = false) where T : Component => ((IEnumerable<T>)gameObject.GetComponentsInParent<T>(includeInactive)).FirstOrDefault<T>();

        public static T GetRequiredComponent<T>(this GameObject gameObject) where T : Component => gameObject.GetComponent<T>();

        public static T GetRequiredComponentInParent<T>(
          this GameObject gameObject,
          bool includeInactive = false)
          where T : Component
        {
            return gameObject.GetComponentInParent<T>(includeInactive);
        }

        public static T GetRequiredComponentInChildren<T>(
          this GameObject gameObject,
          bool includeInactive = false)
          where T : Component
        {
            return gameObject.GetComponentInChildren<T>(includeInactive);
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var toGet = gameObject.GetComponent<T>();
            if (toGet != null) return toGet;
            return gameObject.AddComponent<T>();
        }


        /// <summary>
        /// Get all components of specified Layer in childs
        /// </summary>
        public static List<Transform> GetObjectsOfLayerInChilds(this GameObject gameObject, int layer)
        {
            List<Transform> list = new List<Transform>();
            CheckChildsOfLayer(gameObject.transform, layer, list);
            return list;
        }

        /// <summary>
        /// Get all components of specified Layer in childs
        /// </summary>
        public static List<Transform> GetObjectsOfLayerInChilds(this Component component, int layer)
        {
            return component.gameObject.GetObjectsOfLayerInChilds(layer);
        }

        private static void CheckChildsOfLayer(Transform transform, int layer, List<Transform> childsCache)
        {
            foreach (Transform t in transform)
            {
                CheckChildsOfLayer(t, layer, childsCache);

                if (t.gameObject.layer != layer) continue;
                childsCache.Add(t);
            }
        }

        // CHILD STUFF

        public static void AddChild(this GameObject obj, GameObject child)
        {
            child.transform.SetParent(obj.transform);
        }

        public static void AddChild(this GameObject obj, GameObject child, bool worldPositionStays)
        {
            child.transform.SetParent(obj.transform, worldPositionStays);
        }

        public static GameObject FindChildWithPartialName(
          this GameObject obj,
          string name,
          bool noDive = false)
        {
            GameObject result = null;

            foreach (Transform child in obj.transform)
            {
                if (child.name.StartsWith(name))
                {
                    result = child.gameObject;
                    break;
                }

                if (child.childCount > 0 && !noDive)
                {
                    result = child.gameObject.FindChildWithPartialName(name);
                    if (result != null)
                        break;
                }
            }

            return result;
        }

        public static GameObject FindChild(this GameObject obj, string name, bool dive = false)
        {
            if (!dive)
                return obj.transform.Find(name)?.gameObject;
            else
            {
                GameObject result = null;

                foreach (Transform child in obj?.transform)
                {
                    if (child == null)
                        continue;

                    if (child.name.Equals(name))
                    {
                        result = child.gameObject;
                        break;
                    }

                    if (child.childCount > 0)
                    {
                        result = child.gameObject.FindChild(name, dive);
                        if (result != null)
                            break;
                    }
                }

                return result;
            }
        }

        public static GameObject[] FindChildrenWithPartialName(
          this GameObject obj,
          string name,
          bool noDive = false)
        {
            List<GameObject> result = new List<GameObject>();

            foreach (Transform child in obj.transform)
            {
                if (child.name.StartsWith(name))
                    result.Add(child.gameObject);

                if (child.childCount > 0 && !noDive)
                    result.AddRange(child.gameObject.FindChildrenWithPartialName(name));
            }

            return result.ToArray();
        }

        public static GameObject[] FindChildren(this GameObject obj, string name, bool noDive = false)
        {
            List<GameObject> result = new List<GameObject>();

            foreach (Transform child in obj.transform)
            {
                if (child.name.Equals(name))
                    result.Add(child.gameObject);

                if (child.childCount > 0 && !noDive)
                    result.AddRange(child.gameObject.FindChildren(name));
            }

            return result.ToArray();
        }

        public static GameObject GetChild(this GameObject obj, int index) => obj.transform.GetChild(index).gameObject;

        // PARENT STUFF
        public static T FindComponentInParent<T>(this GameObject obj) where T : Component
        {
            return obj == null ? default : obj.transform.parent?.GetComponent<T>() ?? obj.transform.parent?.gameObject.FindComponentInParent<T>();
        }

        public static bool IsDescendantOf(this GameObject obj, GameObject parent)
        {
            foreach (Transform transform in obj.transform.GetHierarchy())
            {
                if (transform.gameObject.Equals(parent))
                    return true;
            }
            return false;
        }

        // OBTAIN CHILD
        public static GameObject GetChildCopy(this GameObject obj, string name)
        {
            GameObject prefabCopy = obj.CreatePrefabCopy();
            GameObject child = prefabCopy.FindChild(name);
            child.SetActive(false);
            child.transform.SetParent(null);
            GameObjectUtils.Prefabitize(child);
            UnityEngine.Object.Destroy((UnityEngine.Object)prefabCopy);
            return child;
        }

        public static GameObject[] GetChildren(this GameObject obj, bool noDive = false)
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            foreach (Transform transform in obj.transform)
            {
                gameObjectList.Add(transform.gameObject);
                if (transform.childCount > 0 && !noDive)
                    gameObjectList.AddRange(transform.gameObject.GetChildren(noDive));
            }
            return gameObjectList.ToArray();
        }

        // COPY STUFF
        public static GameObject CreatePrefabCopy(this GameObject obj) => PrefabUtils.CopyPrefab(obj);


        public static T CopyComponent<T>(this Component comp, T other) where T : Component
        {
            System.Type type = comp.GetType();
            if (type != other.GetType())
                return default(T);
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (PropertyInfo property in type.GetProperties(bindingAttr))
            {
                if (property.CanWrite)
                {
                    try
                    {
                        property.SetValue((object)comp, property.GetValue((object)other, (object[])null), (object[])null);
                    }
                    catch
                    {
                    }
                }
            }
            foreach (FieldInfo field in type.GetFields(bindingAttr))
                field.SetValue((object)comp, field.GetValue((object)other));
            return comp as T;
        }

        // COMPONENT STUFF
        public static Component[] GetComponents(this GameObject gameObject) => gameObject.GetComponents<Component>().Where(c => c != null).ToArray();

        public static void RemoveComponent<T>(this GameObject go) where T : Component => UnityEngine.Object.Destroy(go.GetComponent<T>());
        public static void RemoveComponentImmediate<T>(this GameObject go) where T : Component => UnityEngine.Object.DestroyImmediate(go.GetComponent<T>());
        public static void RemoveComponent<T>(this GameObject go, T component) where T : Component => UnityEngine.Object.Destroy(component);
        public static void RemoveComponentImmediate<T>(this GameObject go, T component) where T : Component => UnityEngine.Object.DestroyImmediate(component);
        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component => go.AddComponent<T>().GetCopyOf<T>(toAdd);

        /// <summary>
        /// Is the component present in the object?
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <param name="type">The type of component</param>
        /// <returns>True if the component is found, false otherwise</returns>
        public static bool HasComponent(this GameObject obj, System.Type type)
        {
            return obj.GetComponent(type) != null;
        }

        /// <summary>
        /// Is the component present in the object?
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <param name="comp">The component if found, null if not</param>
        /// <typeparam name="T">The type of component</typeparam>
        /// <returns>True if the component is found, false otherwise</returns>
        public static bool HasComponent<T>(this GameObject obj, out T comp) where T : Component
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
        public static bool HasComponent<T>(this GameObject obj) where T : Component
        {
            return obj.GetComponent<T>() != null;
        }

        public static bool HasComponentLong<T>(this GameObject gameObject)
        {
            System.Type ttype = typeof(T);
            Component[] allComponents = gameObject.GetComponents();
            foreach (Component component in allComponents)
            {
                System.Type type = component.GetType();
                if (type == ttype || type.IsSubclassOf(ttype) || ttype.IsAssignableFrom(type))
                    return true;
            }
            return false;
        }

        // SHORTCUTS
        public static void Prefabitize(this GameObject go) => GameObjectUtils.Prefabitize(go);
        public static void Activate(this GameObject obj) => obj.SetActive(true);
        public static void Deactivate(this GameObject obj) => obj.SetActive(false);
        public static bool IsActive(this GameObject obj) => obj.activeSelf;

        // INSTANTIATE INACTIVE
        public static GameObject InstantiateInactive(this GameObject original, bool keepOriginalName = false)
        {
            GameObject clone = GameObjectUtils.InstantiateInactive(original);
            if (keepOriginalName)
                clone.name = original.name;
            return clone;
        }

        public static GameObject InstantiateInactive(this GameObject original, UnityEngine.Transform parent, bool keepOriginalName = false)
        {
            GameObject clone = GameObjectUtils.InstantiateInactive(original, parent);
            if (keepOriginalName)
                clone.name = original.name;
            return clone;
        }

        public static GameObject InstantiateInactive(this GameObject original, UnityEngine.Transform parent, bool worldPositionStays, bool keepOriginalName = false)
        {
            GameObject clone = GameObjectUtils.InstantiateInactive(original, parent, worldPositionStays);
            if (keepOriginalName)
                clone.name = original.name;
            return clone;
        }

        public static GameObject InstantiateInactive(this GameObject original, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, bool keepOriginalName = false)
        {
            GameObject clone = GameObjectUtils.InstantiateInactive(original, position, rotation);
            if (keepOriginalName)
                clone.name = original.name;
            return clone;
        }

        public static GameObject InstantiateInactive(this GameObject original, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation, UnityEngine.Transform parent, bool keepOriginalName = false)
        {
            GameObject clone = GameObjectUtils.InstantiateInactive(original, position, rotation, parent);
            if (keepOriginalName)
                clone.name = original.name;
            return clone;
        }

        //INTERFACES

        /// <summary>
        /// Returns all monobehaviours (casted to T)
        /// </summary>
        /// <typeparam name="T">interface type</typeparam>
        /// <param name="gObj"></param>
        /// <returns></returns>
        public static T[] GetInterfaces<T>(this GameObject gObj)
        {
            if (!typeof(T).IsInterface) throw new System.Exception("Specified type is not an interface!");
            var mObjs = gObj.GetComponents<MonoBehaviour>().Where(c => c != null).ToArray();

            return (from a in mObjs where a.GetType().GetInterfaces().Any(k => k == typeof(T)) select (T)(object)a).ToArray();
        }

        /// <summary>
        /// Returns the first monobehaviour that is of the interface type (casted to T)
        /// </summary>
        /// <typeparam name="T">Interface type</typeparam>
        /// <param name="gObj"></param>
        /// <returns></returns>
        public static T GetInterface<T>(this GameObject gObj)
        {
            if (!typeof(T).IsInterface) throw new System.Exception("Specified type is not an interface!");
            return gObj.GetInterfaces<T>().FirstOrDefault();
        }

        /// <summary>
        /// Returns the first instance of the monobehaviour that is of the interface type T (casted to T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gObj"></param>
        /// <returns></returns>
        public static T GetInterfaceInChildren<T>(this GameObject gObj)
        {
            if (!typeof(T).IsInterface) throw new System.Exception("Specified type is not an interface!");
            return gObj.GetInterfacesInChildren<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all monobehaviours in children that implement the interface of type T (casted to T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gObj"></param>
        /// <returns></returns>
        public static T[] GetInterfacesInChildren<T>(this GameObject gObj)
        {
            if (!typeof(T).IsInterface) throw new System.Exception("Specified type is not an interface!");

            var mObjs = gObj.GetComponentsInChildren<MonoBehaviour>();

            return (from a in mObjs where a.GetType().GetInterfaces().Any(k => k == typeof(T)) select (T)(object)a).ToArray();
        }

        //OTHER
        public static void PrintNamesOfChildren(this GameObject gameObject)
        {
            GameObject[] allChildren = gameObject.GetChildren();
            foreach (GameObject child in allChildren)
            {
                Debug.Log(gameObject.name + "'s child: " + child.name);
            }
        }
        public static void PrintNamesOfComponents(this GameObject gameObject)
        {
            Component[] allComponents = gameObject.GetComponents();
            foreach (Component component in allComponents)
            {
                Debug.Log(gameObject.name + "'s component: " + (component.GetType()).ToString());
            }
        }

        /// <summary>
        /// Set layer to all GameObject children, including inactive.
        /// </summary>
        /// <param name="gameObject">Target GameObject.</param>
        /// <param name="layerNumber">Layer number.</param>
        public static void SetLayerRecursively(this GameObject gameObject, int layerNumber)
        {
            foreach (var trans in gameObject.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = layerNumber;
        }

        /// <summary>
        /// Set layer to all GameObject children, including inactive.
        /// </summary>
        /// <param name="gameObject">Target GameObject.</param>
        /// <param name="layerName">Layer name.</param>
        public static void SetLayerRecursively(this GameObject gameObject, string layerName)
        {
            int layerNumber = LayerMask.NameToLayer(layerName);
            foreach (var trans in gameObject.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = layerNumber;
        }

        /// <summary>
        /// Renderer Bounds of the game object.
        /// </summary>
        /// <param name="go">GameObject you want calculate bounds for.</param>
        /// <returns>Calculated game object bounds.</returns>
        public static Bounds GetRendererBounds(this GameObject go)
        {
            var hasBounds = false;
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            var childrenRenderer = go.GetComponentsInChildren<Renderer>();


            var rnd = go.GetComponent<Renderer>();
            if (rnd != null)
            {
                bounds = rnd.bounds;
                hasBounds = true;
            }

            foreach (var child in childrenRenderer)
                if (!hasBounds)
                {
                    bounds = child.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(child.bounds);
                }

            return bounds;
        }

        /// <summary>
        /// Renderer Bounds of the game object.
        /// </summary>
        /// <param name="go">GameObject you want calculate bounds for.</param>
        /// <returns>Calculated game object bounds.</returns>
        public static Bounds CalculateBounds(this GameObject go) => go.GetRendererBounds();
    }

}