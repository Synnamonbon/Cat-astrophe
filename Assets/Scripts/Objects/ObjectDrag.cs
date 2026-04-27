using System;
using Photon.Pun;
using UnityEngine;

public class ObjectDrag : MonoBehaviourPun, IInteractable
{
    public Vector3 spawnPoint;
    private GameObject holder;
    private GameObject lastHolder;
    private Rigidbody RB;
    [SerializeField] private float eventInvokeDist = 5f;
    private bool farFromHome = false;

    public event Action<int, string> OnDraggedFar;
    
    private void Start()
    {
        spawnPoint = transform.position;
        holder = null;
        lastHolder = null;
        RB = gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(spawnPoint, transform.position) > eventInvokeDist && !farFromHome)
        {
            DraggedFar();
        }
    }
    public void Interact(GameObject go)
    {
        TryToggleBeingHeld(go);
    }

    private void TryToggleBeingHeld(GameObject go)
    {
        if (holder == null)
        {
            gameObject.transform.SetParent(go.transform);
            RB.isKinematic = true;
            holder = go;
        }
        else if (holder == go)
        {
            gameObject.transform.SetParent(null);
            RB.isKinematic = false;
            holder = null;
            lastHolder = go;
        }
    }

    private void DraggedFar()
    {
        Debug.Log("Dragged Far");
        farFromHome = true;
        if (holder != null)
        {
            OnDraggedFar?.Invoke(holder.GetPhotonView().Owner.ActorNumber, gameObject.tag);
        }
        else if (lastHolder != null)
        {
            OnDraggedFar?.Invoke(lastHolder.GetPhotonView().Owner.ActorNumber, gameObject.tag);
        }
        else
        {
            farFromHome = false;
        }
    }
}