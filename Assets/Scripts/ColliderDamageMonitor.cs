using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDamageMonitor : MonoBehaviour
{

    public float damage = 0.0f;
    public bool colliding = false;
    public bool ignoreDamage = false;
    public GameObject lastHitBy = null;
    [Tooltip("The robot to which this damage monitor reports. All damage monitors with the same parent ignore each other. (set automatically)")]
    public GameObject parent; // should be set by whichever script creates this component
    public bool DEBUG = false;
    public void Reset()
    {
        damage = 0.0f;
        colliding = false;
        lastHitBy = null;
    }

    void OnCollisionEnter(Collision collision)
    {
        colliding = true;
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
                lastHitBy = collision.gameObject;
            }
            if (DEBUG)
                Debug.Log(gameObject.name + " hit " + collision.gameObject.name + ". Relative vel: " + collision.relativeVelocity.magnitude + ", Damage is now:" + damage);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (colliding) {
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
            float force = collision.relativeVelocity.magnitude  * Time.fixedDeltaTime;
            if (force > 0.0f)
            {
                // add to damage
                if (!ignoreDamage) {
                    damage += force * 10.0f;
                    lastHitBy = collision.gameObject;
                }
                if (DEBUG)
                    Debug.Log(gameObject.name + " pushing against " + collision.gameObject.name + ". Impulse * dt: " + force + ", Damage is now:" + damage);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        colliding = false;
    }
}
