using System.Runtime.Serialization;

namespace UnityEngine.Serialization
{
    public class Vector4Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector4 vector4 = (Vector4)obj;
            info.AddValue("x", vector4.x);
            info.AddValue("y", vector4.y);
            info.AddValue("z", vector4.z);
            info.AddValue("w", vector4.w);
        }

        public object SetObjectData(
          object obj,
          SerializationInfo info,
          StreamingContext context,
          ISurrogateSelector selector)
        {
            Vector4 vector4 = (Vector4)obj;
            try
            {
                vector4.x = info.GetSingle("x");
                vector4.y = info.GetSingle("y");
                vector4.z = info.GetSingle("z");
                vector4.w = info.GetSingle("w");
            }
            catch
            {
                Debug.LogError("Failed to load vector data");
                vector4.x = 0;
                vector4.y = 0;
                vector4.z = 0;
                vector4.w = 0;
            }
            return vector4;
        }
    }

}