using System.Runtime.Serialization;

namespace UnityEngine.Serialization
{
    public class Vector3Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector3 vector3 = (Vector3)obj;
            info.AddValue("x", vector3.x);
            info.AddValue("y", vector3.y);
            info.AddValue("z", vector3.z);
        }

        public object SetObjectData(
          object obj,
          SerializationInfo info,
          StreamingContext context,
          ISurrogateSelector selector)
        {
            Vector3 vector3 = (Vector3)obj;
            try
            {
                vector3.x = info.GetSingle("x");
                vector3.y = info.GetSingle("y");
                vector3.z = info.GetSingle("z");
            }
            catch
            {
                Debug.LogError("Failed to load vector data");
                vector3.x = 0;
                vector3.y = 0;
                vector3.z = 0;
            }
            return vector3;
        }
    }

    public class Vector3IntSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector3Int vector3 = (Vector3Int)obj;
            info.AddValue("x", vector3.x);
            info.AddValue("y", vector3.y);
            info.AddValue("z", vector3.z);
        }

        public object SetObjectData(
          object obj,
          SerializationInfo info,
          StreamingContext context,
          ISurrogateSelector selector)
        {
            Vector3Int vector3 = (Vector3Int)obj;
            try
            {
                vector3.x = info.GetInt32("x");
                vector3.y = info.GetInt32("y");
                vector3.z = info.GetInt32("z");
            }
            catch
            {
                Debug.LogError("Failed to load vector data");
                vector3.x = 0;
                vector3.y = 0;
                vector3.z = 0;
            }
            return vector3;
        }
    }
}