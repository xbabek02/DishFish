using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Sea : MonoBehaviour
{
    public float floatForce = 10f;     // Upward force strength
    public float damping = 1f;         // Reduces bouncing
    public float waterSurfaceY = 42f;   // Surface height

    private List<Rigidbody> bodiesInWater = new List<Rigidbody>();
    

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !bodiesInWater.Contains(rb))
        {
            bodiesInWater.Add(rb);
        }
        if (other.gameObject.layer == 6) //hooklayer
        {
            Hook hook = other.gameObject.GetComponent<Hook>();
            hook.fishingRod.startFishing();
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            bodiesInWater.Remove(rb);
        }
    }

    void FixedUpdate()
    {
        foreach (Rigidbody rb in bodiesInWater)
        {
            if (rb.IsDestroyed() || !rb) continue;

            float depth = waterSurfaceY - rb.position.y;

            if (depth > 0f)
            {
                // Upward buoyancy force
                rb.AddForce(Vector3.up * (floatForce * depth), ForceMode.Acceleration);

                // Optional damping to reduce jitter
                rb.AddForce(-rb.linearVelocity * damping, ForceMode.Acceleration);
            }
        }
    }
}