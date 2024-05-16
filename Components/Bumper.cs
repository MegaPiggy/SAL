using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SALT
{
    public class BounceBackBumber : MonoBehaviour
    {
        [SerializeField] private float bounceForce = 1;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 8)
            {
                ContactPoint2D[] contacts = new ContactPoint2D[10];

                collision.GetContacts(contacts);

                Rigidbody2D otherRB = collision.attachedRigidbody;
                otherRB.AddForce(contacts[0].normal * bounceForce);

                Vector3 dir = collision.transform.position - transform.position;
                dir.y = 0;
                dir = dir.normalized;
                otherRB.AddForce(dir * bounceForce);
                Debug.LogError("Here");
            }
        }
    }
}