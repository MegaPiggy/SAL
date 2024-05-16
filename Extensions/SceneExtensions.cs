using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SALT.Extensions
{
    /// <summary>
    /// Extensions for the Scene class.
    /// </summary>
    public static class SceneExtensions
    {
        public static bool IsPlaying(this Scene scene) => !(!Application.isPlaying && !scene.isLoaded);

        public static bool IsInteractable(this Scene scene) => scene.IsValid() && scene.IsPlaying();

        public static bool IsInteractable(this Scene scene, out string reason)
        {
            if (!scene.IsValid())
            {
                reason = "The scene is invalid.";
                return false;
            }

            if (!scene.IsPlaying())
            {
                reason = "The scene is not loaded.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Returns the components of Type `type` located on the scene root GameObjects or any of their children using depth search.
        /// Components are returned only if it is found on any active GameObject.
        /// </summary>
        /// <param name="scene">Scene to operate with.</param>
        /// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <returns>Components of the matching type, if found.</returns>
        public static IEnumerable<T> GetComponentsInChildren<T>(this Scene scene, bool includeInactive = false) where T : class
        {
            if (!scene.IsInteractable())
                return Enumerable.Empty<T>();
            return scene.GetRootGameObjects().SelectMany(gameObject => gameObject.GetComponentsInChildren<T>(includeInactive));
        }

        /// <summary>
        /// Returns the component of Type `type` located on one of the scene root GameObjects or any of their children using depth first search.
        /// A component is returned only if it is found on an active GameObject.
        /// </summary>
        /// <param name="scene">Scene to operate with.</param>
        /// <param name="includeInactive">Should Components on inactive GameObjects be included in the found set?</param>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <returns>A component of the matching type, if found.</returns>
        public static T GetComponentInChildren<T>(this Scene scene, bool includeInactive = false) where T : class
        {
            if (!scene.IsInteractable())
                return default;

            foreach (var gameObject in scene.GetRootGameObjects())
            {
                var component = gameObject.GetComponentInChildren<T>(includeInactive);
                if (component != null)
                    return component;
            }

            return default;
        }

        /// <summary>
        /// Returns the components of Type `type` located on the scene root GameObjects.
        /// Components are returned only if it is found on any active GameObject.
        /// </summary>
        /// <param name="scene">Scene to operate with.</param>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <returns>Components of the matching type, if found.</returns>
        public static IEnumerable<T> GetComponents<T>(this Scene scene) where T : class
        {
            if (!scene.IsInteractable())
                return Enumerable.Empty<T>();
            return scene.GetRootGameObjects().SelectMany(gameObject => gameObject.GetComponents<T>().Cast<Component>()).Where(c => c != null).Cast<T>();
        }

        /// <summary>
        /// Returns the component of Type `type` located on one of the scene root GameObjects.
        /// A component is returned only if it is found on an active GameObject.
        /// </summary>
        /// <param name="scene">Scene to operate with.</param>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <returns>A component of the matching type, if found.</returns>
        public static T GetComponent<T>(this Scene scene) where T : class
        {
            if (!scene.IsInteractable())
                return default;

            foreach (var gameObject in scene.GetRootGameObjects())
            {
                var component = gameObject.GetComponent<T>();
                if (component != null)
                    return component;
            }

            return default;
        }

        /// <summary>
        /// Unload this Scene.
        /// </summary>
        /// <param name="self">A Scene instance.</param>
        public static void Unload(this Scene self)
        {
            if (self.isLoaded && self.IsValid())
                SceneManager.UnloadSceneAsync(self.name);
        }

        /// <summary>
        /// Places a GameObject in this Scene.
        /// </summary>
        /// <param name="self">A Scene instance.</param>
        /// <param name="o">An existing object.</param>
        public static void Add(this Scene self, GameObject o)
        {
            if (!self.IsInteractable())
                return;

            if (o != null)
                SceneManager.MoveGameObjectToScene(o, self);
        }

        /// <summary>
        /// Instantiate a GameObject and place it in this Scene.
        /// </summary>
        /// <param name="self">A Scene instance.</param>
        /// <param name="original">An existing object that you want to make a copy of.</param>
        /// <param name="keepOriginalName">Whether to keep the <paramref name="original"/>'s name or not</param>
        /// <returns>The instantiated GameObject.</returns>
        public static GameObject Instantiate(this Scene self, GameObject original, bool keepOriginalName = false)
        {
            GameObject o = original.Instantiate(keepOriginalName);

            if (!self.IsInteractable())
                return o;

            if (o != null)
            {
                SceneManager.MoveGameObjectToScene(o, self);
                return o;
            }

            return null;
        }

        /// <summary>
        /// Instantiate a GameObject and place it in this Scene.
        /// </summary>
        /// <param name="self">A Scene instance.</param>
        /// <param name="original">An existing object that you want to make a copy of.</param>
        /// <param name="keepOriginalName">Whether to keep the <paramref name="original"/>'s name or not</param>
        /// <returns>The instantiated GameObject.</returns>
        public static GameObject InstantiateInactive(this Scene self, GameObject original, bool keepOriginalName = false)
        {
            GameObject o = original.InstantiateInactive(keepOriginalName);

            if (!self.IsInteractable())
                return o;

            if (o != null)
            {
                SceneManager.MoveGameObjectToScene(o, self);
                return o;
            }

            return null;
        }

        /// <summary>
        /// Finds a GameObject in a given scene.
        /// </summary>
        /// <param name="scene">The scene to search in</param>
        /// <param name="name">The name of the GameObject</param>
        /// <returns>The found GameObject, null if none is found.</returns>
        public static GameObject Find(this Scene scene, string name)
        {
            if (scene.IsInteractable())
            {
                var rootGos = scene.GetRootGameObjects();
                foreach (GameObject gos in rootGos)
                {
                    if (gos.name == name)
                        return gos;
                    
                    GameObject retGo = gos.FindChild(name, true);
                    if (retGo != null)
                        return retGo;
                }
            }

            return null;
        }
    }
}
