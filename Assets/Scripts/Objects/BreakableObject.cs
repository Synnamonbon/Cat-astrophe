using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Range(0f, 10f)]
    [SerializeField] private float breakSpeed = 0.1f;
    [Range(0.2f, 10f)]
    [SerializeField] private float timeBeforeDespawn = 2f;
    [Range(0.01f, 2f)]
    [SerializeField] private float fragmentDespawnSpeed = 0.25f;
    [Header("Drag and drop the fractured object PREFAB below:")]
    [SerializeField] public GameObject fractured;
    [Header("Transparent material goes below here:")]
    [SerializeField] private Material TRANSPARENT_MATERIAL_SRC;
    private int GROUND_LAYER;
    private Rigidbody RIGIDBODY;

    public void Awake()
    {
        GROUND_LAYER = LayerMask.NameToLayer("Ground");
        RIGIDBODY = gameObject.GetComponent<Rigidbody>();
    }

    private void BreakObject()
    {
        if (RIGIDBODY != null)
        {
            Destroy(RIGIDBODY);
        }
        if (TryGetComponent<Collider>(out Collider col))
        {
            col.enabled = false;
        }
        if (TryGetComponent<Renderer>(out Renderer r))
        {
            r.enabled = false;
        }

        GameObject brokenInstance = Instantiate(fractured, transform.position, transform.rotation);
        Rigidbody[] rbs = brokenInstance.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody b in rbs)
        {
            b.linearVelocity = RIGIDBODY.linearVelocity;
        }

        StartCoroutine(DespawnFragments(rbs, brokenInstance));
    }

    private IEnumerator DespawnFragments(Rigidbody[] rbs, GameObject brokenInstance)
    {
        WaitForSeconds wait = new WaitForSeconds(timeBeforeDespawn);
        int activeRBS = rbs.Length;
        //Debug.Log(activeRBS);

        while(activeRBS > 0)
        {
            yield return wait;
            //Debug.Log("Waiting");
            foreach(Rigidbody rb in rbs)
            {
                if (rb.IsSleeping())
                {
                    activeRBS--;
                    //Debug.Log(activeRBS);
                }
            }
        }

        float t = 0f;
        Renderer[] renderers = Array.ConvertAll(rbs, GetRendererFromRB);

        foreach (Rigidbody rb in rbs)
        {
            Destroy(rb.GetComponent<Collider>());
            Destroy(rb);
        }
        renderers = Array.ConvertAll(renderers, ReplaceWithTransparent);

        while (t < 1)
        {
            float step = Time.deltaTime * fragmentDespawnSpeed;
            foreach (Renderer renderer in renderers)
            {
                Color c = renderer.material.color;
                renderer.material.color = new Color(c.r, c.g, c.b, c.a - step);
            }

            t += step;
            yield return null;
        }

        Debug.Log(renderers[0].material.color);

        Destroy(brokenInstance);
        Destroy(gameObject);
        yield return null;
    }

    public void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.layer);
        // if collision has tag ground
        if (collision.gameObject.layer != GROUND_LAYER){return;}
        // if velocity is above a certain threshold
        float vel = RIGIDBODY.linearVelocity.magnitude;
        //Debug.Log(vel);
        if (vel > breakSpeed)
        {    
            BreakObject();
        }
    }

    private Renderer GetRendererFromRB(Rigidbody rb)
    {
        return rb.GetComponent<Renderer>();
    }

    private Renderer ReplaceWithTransparent(Renderer ren)
    {
        ren.material = new Material(TRANSPARENT_MATERIAL_SRC);
        return ren;
    }
}
