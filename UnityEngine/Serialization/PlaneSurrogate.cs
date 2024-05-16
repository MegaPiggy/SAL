using System.Runtime.Serialization;

namespace UnityEngine.Serialization
{
    public class PlaneSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Plane plane = (Plane)obj;
            info.AddValue("normal.x", plane.normal.x);
            info.AddValue("normal.y", plane.normal.y);
            info.AddValue("normal.z", plane.normal.z);
            info.AddValue("distance", plane.distance);
        }

        public object SetObjectData(
          object obj,
          SerializationInfo info,
          StreamingContext context,
          ISurrogateSelector selector)
        {
            Plane plane = (Plane)obj;
            try
            {
                plane.normal = new Vector3(info.GetSingle("normal.x"), info.GetSingle("normal.y"), info.GetSingle("normal.z"));
                plane.distance = info.GetSingle("distance");
            }
            catch
            {
                Debug.LogError("Failed to load plane data");
                plane.normal = Vector3.zero;
                plane.distance = 0;
            }
            return plane;
        }
    }
}
