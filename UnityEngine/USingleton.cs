namespace UnityEngine
{
    /// <summary>
    /// Makes a unity singleton class, said class will only allow a single instance of itself (is child of Mono Behaviour)
    /// </summary>
    /// <typeparam name="T">The type of the class turned into a singleton</typeparam>
    public abstract class USingleton<T> : MonoBehaviour where T : USingleton<T>, new()
    {
        /// <summary>The instance of this singleton</summary>
        public static T Instance { get; protected set; }

        /// <summary>Awakes the script</summary>
        protected virtual void Awake()
        {
            if ((Object)USingleton<T>.Instance != (Object)null)
                Object.Destroy((Object)this);
            USingleton<T>.Instance = (T)this;
            Object.DontDestroyOnLoad((Object)this);
        }
    }
}
