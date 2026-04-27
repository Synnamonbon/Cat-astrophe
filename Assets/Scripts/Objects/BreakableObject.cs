using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class BreakableObject : MonoBehaviourPunCallbacks, IInteractable
{
    [SerializeField] private BreakableObject_SO breakableObjectData;

    private int GROUND_LAYER;
    private Rigidbody RIGIDBODY;
    private PhotonRigidbodyView RIGIDBODY_VIEW;
    private int ackCounter;

    public event Action<int, ObjectType, Vector3, string> OnBreak;           // playerID, Object Type, Position, Tag
    public event Action<int, int> OnBreakOnNPC;             // playerID, enemy photonview ID
    
    public void Awake()
    {
        GROUND_LAYER = LayerMask.NameToLayer("Ground");
        RIGIDBODY = gameObject.GetComponent<Rigidbody>();
        RIGIDBODY_VIEW = gameObject.GetComponent<PhotonRigidbodyView>();
    }

    public void Start()
    {
        if (breakableObjectData.ObjectTag != "")
        {
            gameObject.tag = breakableObjectData.ObjectTag;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;
        // Debug.Log(collision.gameObject.layer);
        // if velocity is above a certain threshold
        float vel = RIGIDBODY.linearVelocity.magnitude;
        // if collision has tag ground
        if (collision.gameObject.layer == GROUND_LAYER)
        {
            //Debug.Log(vel);
            if (vel > breakableObjectData.BreakSpeed)
            {   
                photonView.RPC(nameof(CreateBrokenObject), RpcTarget.All);
                StartCoroutine(DestroyOriginalObject());
            }
        }
        else if (collision.gameObject.tag == "EnemyNPC")
        {
            if (vel > 0)
            {
                if(collision.gameObject.TryGetComponent<PhotonView>(out PhotonView pv))
                {
                    OnBreakOnNPC?.Invoke(photonView.Owner.ActorNumber, pv.ViewID);
                }
                photonView.RPC(nameof(CreateBrokenObject), RpcTarget.All);
                StartCoroutine(DestroyOriginalObject());
            }
        }
    }

    [PunRPC]
    private void CreateBrokenObject()
    {
        if (RIGIDBODY != null && RIGIDBODY_VIEW != null)
        {
            Destroy(RIGIDBODY_VIEW);
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

        // Check if I am master client, take responsibility to alert enemies
        if (PhotonNetwork.IsMasterClient)
        {
            AlertManager.instance.AlertNPCsInRange(transform, breakableObjectData.AlertDetectionDistance);
            // Invoke your own OnBreak event passing your photonView ownerID and EnumObjectType objectType
            OnBreak?.Invoke(photonView.Owner.ActorNumber, breakableObjectData.ObjectType, RIGIDBODY.transform.position, gameObject.tag);
        }

        GameObject brokenInstance = Instantiate (breakableObjectData.Fractured, transform.position, transform.rotation);
        Rigidbody[] rbs = brokenInstance.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody b in rbs)
        {
            b.linearVelocity = RIGIDBODY.linearVelocity;
        }
        StartCoroutine(DespawnFragments(rbs, brokenInstance));
    }

    private IEnumerator DespawnFragments(Rigidbody[] rbs, GameObject brokenInstance)
    {
        WaitForSeconds wait = new WaitForSeconds(breakableObjectData.TimeBeforeDespawn);
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
            float step = Time.deltaTime * breakableObjectData.FragmentDespawnSpeed;
            foreach (Renderer renderer in renderers)
            {
                Color c = renderer.material.color;
                renderer.material.color = new Color(c.r, c.g, c.b, c.a - step);
            }

            t += step;
            yield return null;
        }
        //Debug.Log(renderers[0].material.color);

        Destroy(brokenInstance);
        photonView.RPC(nameof(SendAcknowledgement), RpcTarget.All);
        yield return null;
    }

     [PunRPC]
    private void SendAcknowledgement()
    {
        if (photonView.IsMine)
        {
            ackCounter ++;
        }
    }

    private Renderer GetRendererFromRB(Rigidbody rb)
    {
        return rb.GetComponent<Renderer>();
    }

    private Renderer ReplaceWithTransparent(Renderer ren)
    {
        ren.material = new Material(breakableObjectData.TRANSPARENT_MATERIAL_SRC);
        return ren;
    }

    private IEnumerator DestroyOriginalObject()
    {
        yield return new WaitUntil(() => ackCounter == PhotonNetwork.PlayerList.Length);
        InteractableManager.instance.DestroyForAll(gameObject);
    }

    public void Interact(GameObject go)
    {
        Debug.Log($"Me when I ({go.name}) lick the breakable object or smtn idk");
    }
}
