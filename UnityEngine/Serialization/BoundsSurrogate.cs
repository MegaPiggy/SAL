using System.Runtime.Serialization;

namespace UnityEngine.Serialization
{
    public class BoundsSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Bounds bounds = (Bounds)obj;
            info.AddValue("center.x", bounds.center.x);
            info.AddValue("center.y", bounds.center.y);
            info.AddValue("center.z", bounds.center.z);
            info.AddValue("size.x", bounds.size.x);
            info.AddValue("size.y", bounds.size.y);
            info.AddValue("size.z", bounds.size.z);
        }

        public object SetObjectData(
          object obj,
          SerializationInfo info,
          StreamingContext context,
          ISurrogateSelector selector)
        {
            Bounds bounds = (Bounds)obj;
            try
            {
                bounds.center = new Vector3(info.GetSingle("center.x"), info.GetSingle("center.y"), info.GetSingle("center.z"));
                bounds.size = new Vector3(info.GetSingle("size.x"), info.GetSingle("size.y"), info.GetSingle("size.z"));
            }
            catch
            {
                Debug.LogError("Failed to load bounds data");
                bounds.center = Vector3.zero;
                bounds.size = Vector3.one;
            }
            return bounds;
        }
    }

    public class BoundsIntSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            BoundsInt bounds = (BoundsInt)obj;
            info.AddValue("x", bounds.x);
            info.AddValue("y", bounds.y);
            info.AddValue("z", bounds.z);
            info.AddValue("size.x", bounds.size.x);
            info.AddValue("size.y", bounds.size.y);
            info.AddValue("size.z", bounds.size.z);
        }

        public object SetObjectData(
          object obj,
          SerializationInfo info,
          StreamingContext context,
          ISurrogateSelector selector)
        {
            BoundsInt bounds = (BoundsInt)obj;
            try
            {
                bounds.x = info.GetInt32("x");
                bounds.y = info.GetInt32("y");
                bounds.z = info.GetInt32("z");
                bounds.size = new Vector3Int(info.GetInt32("size.x"), info.GetInt32("size.y"), info.GetInt32("size.z"));
            }
            catch
            {
                Debug.LogError("Failed to load bounds data");
                bounds.x = 0;
                bounds.y = 0;
                bounds.z = 0;
                bounds.size = Vector3Int.one;
            }
            return bounds;
        }
    }
}