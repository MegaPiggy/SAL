using UnityEngine;
using SALT.Extensions;

namespace SALT
{
    /// <summary>
    /// A way to differentiate which character is which.
    /// </summary>
    public class CharacterIdentifiable : MonoBehaviour
    {
        private Character idnt = Character.NONE;

        /// <summary>
        /// The current <see cref="Character"/> id of the <see cref="CharacterPack"/>
        /// </summary>
        public Character Id { get => idnt; set => SetId(value); }

        /// <summary>
        /// Sets the <see cref="Id"/>
        /// </summary>
        /// <param name="enumValue">The value the id will be set to</param>
        public void SetId(Character enumValue) => idnt = enumValue;

        /// <summary>
        /// Sets the <see cref="Id"/>
        /// </summary>
        /// <param name="enumValue">An <see cref="int"/> equal to the <see cref="Character"/> id</param>
        public void SetId(int enumValue) => idnt = SALT.Registries.CharacterRegistry.GetFromInt(enumValue);

        /// <summary>
        /// Gets the <see cref="Id"/>
        /// </summary>
        /// <returns></returns>
        public Character GetId() => idnt;

        /// <summary>
        /// Gets the <see cref="Id"/> from the <see cref="GameObject"/>
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Character GetId(GameObject b)
        {
            if (b == null)
                return Character.NONE;
            CharacterIdentifiable identifiable = b.GetComponent<CharacterIdentifiable>();
            if (identifiable == null)
                return Character.NONE;
            return identifiable.idnt;
        }

        /// <summary>
        /// Adds a <see cref="CharacterIdentifiable"/> to a <see cref="GameObject"/>
        /// </summary>
        /// <param name="b"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static void AddIdentifiable(GameObject b, Character id) => b.GetOrAddComponent<CharacterIdentifiable>().Id = id;

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }
        public virtual void OnApplicationQuit() { }
    }
}
