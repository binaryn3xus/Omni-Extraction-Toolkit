using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using OmniExtractionToolkit;

namespace OmniExtractionToolkit.Features
{
    public class CartShrinkComponent : MonoBehaviour
    {
        private PhysGrabObject pObj;
        private PhysGrabObjectImpactDetector impactDetector;
        private RoomVolumeCheck roomVolumeCheck;
        private Vector3 originalScale;
        private Vector3 originalDetectionSize;
        private bool isShrunk = false;
        private bool massModified = false;
        private Rigidbody rb;
        private float originalDrag;
        private float originalAngularDrag;
        private List<Collider> colliders = new List<Collider>();
        private static List<PhysGrabCart> activeCarts = new List<PhysGrabCart>();
        private static float lastCartScan = 0f;

        private void Start()
        {
            pObj = GetComponent<PhysGrabObject>();
            rb = GetComponent<Rigidbody>();
            impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
            roomVolumeCheck = GetComponent<RoomVolumeCheck>();
            
            originalScale = transform.localScale;
            
            if (roomVolumeCheck != null)
            {
                originalDetectionSize = roomVolumeCheck.currentSize;
            }

            if (rb != null)
            {
                originalDrag = rb.drag;
                originalAngularDrag = rb.angularDrag;
            }

            // Cache colliders to modify bounciness later
            colliders.AddRange(GetComponentsInChildren<Collider>());
        }

        private void Update()
        {
            if (pObj == null || pObj.dead) return;

            // Only the Host manages physical transformations to ensure lobby consistency.
            // In single-player (not in a room), this will also allow the transformation.
            if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) return;

            // 1. Periodically update list of carts on the map
            if (Time.time - lastCartScan > 3f)
            {
                activeCarts.Clear();
                activeCarts.AddRange(FindObjectsOfType<PhysGrabCart>());
                lastCartScan = Time.time;
            }

            // 2. Check if we are inside any cart's storage area
            bool inAnyCart = false;
            foreach (var cart in activeCarts)
            {
                if (cart == null || cart.gameObject == null) continue;

                Transform storageArea = Traverse.Create(cart).Field<Transform>("inCart").Value;
                if (storageArea == null) continue;

                if (Vector3.Distance(transform.position, storageArea.position) > 6f) continue;

                Vector3 localPos = storageArea.InverseTransformPoint(transform.position);
                float fieldScale = OmniExtractionToolkitPlugin.CartShrinkFieldSize.Value;

                if (Mathf.Abs(localPos.x) <= 1.2f * fieldScale && Mathf.Abs(localPos.z) <= 1.5f * fieldScale && localPos.y >= -0.5f * fieldScale && localPos.y <= 5f * fieldScale)
                {
                    inAnyCart = true;
                    break;
                }
            }

            // 3. Apply/Reset Shrink (Works even if held!)
            if (OmniExtractionToolkitPlugin.EnableCartShrink.Value && inAnyCart)
            {
                ApplyShrink();
            }
            else if (isShrunk)
            {
                ResetScale();
            }

            // 4. Apply/Reset Weight Reduction (Linked to Shrink Factor)
            if (OmniExtractionToolkitPlugin.EnableWeightReduction.Value && inAnyCart)
            {
                ApplyWeightReduction();
            }
            else if (massModified)
            {
                ResetMass();
            }
        }

        private void ApplyShrink()
        {
            float factor = OmniExtractionToolkitPlugin.CartShrinkFactor.Value;
            float speed = OmniExtractionToolkitPlugin.CartShrinkSpeed.Value;
            Vector3 targetScale = originalScale * factor;
            
            if (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
                
                // Sync Detection Field Size
                if (roomVolumeCheck != null)
                {
                    roomVolumeCheck.currentSize = originalDetectionSize * (transform.localScale.x / originalScale.x);
                }
                
                isShrunk = true;
            }
        }

        private void ResetScale()
        {
            float speed = OmniExtractionToolkitPlugin.CartShrinkSpeed.Value;
            if (Vector3.Distance(transform.localScale, originalScale) > 0.001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * speed);
                
                // Sync Detection Field Size
                if (roomVolumeCheck != null)
                {
                    roomVolumeCheck.currentSize = originalDetectionSize * (transform.localScale.x / originalScale.x);
                }
            }
            else
            {
                transform.localScale = originalScale;
                if (roomVolumeCheck != null) roomVolumeCheck.currentSize = originalDetectionSize;
                isShrunk = false;
            }
        }

        private void ApplyWeightReduction()
        {
            if (rb != null)
            {
                // LINKED: Weight uses the same factor as shrinking
                float factor = OmniExtractionToolkitPlugin.CartShrinkFactor.Value;
                float targetMass = pObj.massOriginal * factor;
                if (Mathf.Abs(rb.mass - targetMass) > 0.01f)
                {
                    rb.mass = targetMass;
                    
                    // COUNTER-BOUNCE: Increase drag significantly to compensate for lower mass and smaller size.
                    // We use an even higher multiplier (5.0) to combat scripted impulses.
                    rb.drag = originalDrag + (1.0f / factor) * 5.0f;
                    rb.angularDrag = originalAngularDrag + (1.0f / factor) * 5.0f;
                    
                    // Kill natural bounciness of all colliders
                    foreach (var col in colliders)
                    {
                        if (col != null && col.material != null)
                        {
                            col.material.bounciness = 0f;
                            col.material.bounceCombine = PhysicMaterialCombine.Minimum;
                        }
                    }

                    massModified = true;
                }
            }
        }

        private void FixedUpdate()
        {
            // EXTRA DAMPENING: For shrunk items, manually cap velocity to prevent "super-bounces" 
            // caused by scripts that add fixed impulse forces (like the square basketball).
            if (isShrunk && rb != null && !pObj.grabbed)
            {
                float maxVel = 1.5f; // Tightened from 4.0
                if (rb.velocity.magnitude > maxVel)
                {
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVel);
                }
            }
        }

        private void ResetMass()
        {
            if (rb != null)
            {
                rb.mass = pObj.massOriginal;
                rb.drag = originalDrag;
                rb.angularDrag = originalAngularDrag;

                // Restore bounciness
                foreach (var col in colliders)
                {
                    if (col != null && col.material != null)
                    {
                        col.material.bounciness = 0.4f; 
                        col.material.bounceCombine = PhysicMaterialCombine.Average;
                    }
                }

                massModified = false;
            }
        }
    }

    [HarmonyPatch(typeof(PhysGrabObject), "Start")]
    public static class CartShrink_Patch
    {
        static void Postfix(PhysGrabObject __instance)
        {
            bool isLoot = __instance.GetComponent<ValuableObject>() != null || 
                         __instance.GetComponent<ValuableDiscoverCustom>() != null;

            if (!isLoot) return;

            if (__instance.gameObject.GetComponent<CartShrinkComponent>() == null)
            {
                __instance.gameObject.AddComponent<CartShrinkComponent>();
            }
        }
    }
}
