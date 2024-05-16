using System.Runtime.Serialization;

namespace UnityEngine.Serialization
{
    public class Vector2Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector2 vector2 = (Vector2)obj;
            info.AddValue("x", vector2.x);
            info.AddValue("y", vector2.y);
        }

        public object SetObjectData(
          object obj,
          SerializationInfo info,
          StreamingContext context,
          ISurrogateSelector selector)
        {
            Vector2 vector2 = (Vector2)obj;
            try
            {
                vector2.x = info.GetSingle("x");
                vector2.y = info.GetSingle("y");
            }
            catch
            {
                Debug.LogError("Failed to load vector data");
                vector2.x = 0;
                vector2.y = 0;
            }
            return vector2;
        }
    }

    public class Vector2IntSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector2Int vector2 = (Vector2Int)obj;
            info.AddValue("x", vector2.x);
            info.AddValue("y", vector2.y);
        }

        public object SetObjectData(
          object obj,
          SerializationInfo info,
          StreamingContext context,
          ISurrogateSelector selector)
        {
            Vector2Int vector2 = (Vector2Int)obj;
            try
            {
                vector2.x = info.GetInt32("x");
                vector2.y = info.GetInt32("y");
            }
            catch
            {
                Debug.LogError("Failed to load vector data");
                vector2.x = 0;
                vector2.y = 0;
            }
            return vector2;
        }
    }
}