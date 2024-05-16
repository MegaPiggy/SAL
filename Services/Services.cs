using SALT.Utils;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SALT.Extensions;

namespace SALT
{
    /// <summary>This class controls all services that guu runs, use it to activate the services your mod needs.</summary>
    public static class Services
    {
        private const string SYSTEM_OBJECT_NAME = "_SALTServices";
        private static bool internalServicesInit;
        internal static GameObject servicesObj;

        internal static void InitInternalServices()
        {
            if (internalServicesInit)
                return;
            foreach (System.Type type in TypeUtils.GetChildsOf<IService>(Main.execAssembly))
            {
                if (type.IsSubclassOf(typeof(Component)) && !servicesObj.HasComponent(type))
                {
                    if (typeof(IServiceInternal).IsAssignableFrom(type))
                    {
                        servicesObj.AddComponent(type);
#if DEBUG
                        Debug.Log("- Registered internal service '" + type.Name + "'");
#endif
                    }
                    else
                    {
                        if (servicesObj.AddComponent(type) is Behaviour behaviour)
                            behaviour.enabled = false;
#if DEBUG
                        Debug.Log("- Registered service '" + type.Name + "'");
#endif
                    }
                }
            }
            internalServicesInit = true;
        }

        internal static void CreateServiceObject()
        {
            servicesObj = new GameObject(SYSTEM_OBJECT_NAME);
            servicesObj.DontDestroyOnLoad();
#if DEBUG
            Debug.Log("- Initializing service object");
#endif
        }
    }
}
