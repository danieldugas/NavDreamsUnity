using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDamageMonitor : MonoBehaviour
{

    public float damage = 0.0f;
    public bool ignoreDamage = false;
    [Tooltip("The robot to which this damage monitor reports. All damage monitors with the same parent ignore each other. (set automatically)")]
    public GameObject parent; // should be set by whichever script creates this component
    public bool DEBUG = false;
    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Debug.DrawRay(contact.point, contact.normal, Color.white);
        }
        // we ignore wheels
        if (gameObject.name == "FrontRightWheel" || gameObject.name == "FrontLeftWheel" || gameObject.name == "RearWheel")
        {
            return;
        }
        // ignore collisions with other damage monitors from same robot
        if (collision.gameObject.GetComponent<ColliderDamageMonitor>() != null && collision.gameObject.GetComponent<ColliderDamageMonitor>().parent == parent)
        {
            return;
        }
        // add to damage
        if (collision.relativeVelocity.magnitude > 0.01f)
        {
            if (!ignoreDamage) {
                damage += collision.relativeVelocity.magnitude;
            }
            if (DEBUG)
                Debug.Log(gameObject.name + " hit by " + collision.gameObject.name + ": " + collision.relativeVelocity.magnitude + " " + damage);
        }
    }
}
