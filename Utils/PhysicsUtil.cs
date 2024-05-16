using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SALT.Extensions;

public static class PhysicsUtil
{
    public static float RadiusOfObject(GameObject obj)
    {
        float a = 0.0f;
        foreach (Collider component in obj.GetComponents<Collider>())
        {
            if (component != null && !component.isTrigger)
            {
                if (obj.activeInHierarchy)
                {
                    Bounds bounds = component.bounds;
                    a = Mathf.Max(a, (float)(0.5 * (double)Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z)));
                }
                else
                    a = Mathf.Max(a, PhysicsUtil.CalcRad(component));
            }
        }
        return a;
    }

    public static float CalcRad(Collider col)
    {
        Vector3 lossyScale = col.transform.lossyScale;
        switch (col)
        {
            case SphereCollider _:
                return (float)((double)((SphereCollider)col).radius * (double)Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z));
            case BoxCollider _:
                BoxCollider boxCollider = (BoxCollider)col;
                return Mathf.Max(boxCollider.size.x * lossyScale.x, boxCollider.size.y * lossyScale.y, boxCollider.size.z * lossyScale.z) * 0.5f;
            case CapsuleCollider _:
                CapsuleCollider capsuleCollider = (CapsuleCollider)col;
                float num1 = capsuleCollider.direction == 0 ? capsuleCollider.height * 0.5f : capsuleCollider.radius;
                float num2 = capsuleCollider.direction == 1 ? capsuleCollider.height * 0.5f : capsuleCollider.radius;
                float num3 = capsuleCollider.direction == 2 ? capsuleCollider.height * 0.5f : capsuleCollider.radius;
                return Mathf.Max(num1 * lossyScale.x, num2 * lossyScale.y, num3 * lossyScale.z);
            default:
                return 0.0f;
        }
    }

    public static void IgnoreCollision(GameObject a, GameObject b, bool ignored = true)
    {
        Collider[] componentsInChildren1 = a.GetComponentsInChildren<Collider>();
        Collider[] componentsInChildren2 = b.GetComponentsInChildren<Collider>();
        foreach (Collider collider1 in componentsInChildren1)
        {
            foreach (Collider collider2 in componentsInChildren2)
                Physics.IgnoreCollision(collider1, collider2, ignored);
        }
    }

    private static IEnumerator RestoreCollision(
      GameObject a,
      GameObject b,
      float enableAfter)
    {
        yield return (object)new WaitForSeconds(enableAfter);
        if ((Object)a != (Object)null && (Object)b != (Object)null)
            PhysicsUtil.IgnoreCollision(a, b, false);
    }
}
