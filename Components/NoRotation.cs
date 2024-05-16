using UnityEngine;

namespace SALT
{
    internal class NoRotation : MonoBehaviour
    {
        public void Update()
        {
            this.gameObject.transform.rotation = new Quaternion(0, 0, 0, 1);
        }
    }
}
