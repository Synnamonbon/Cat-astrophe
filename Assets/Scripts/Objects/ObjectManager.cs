using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public static ObjectManager instance;

    [SerializeField] private GameObject objectPrefab;
    private List<Transform> spawnPoints = new List<Transform>();
    public event Action<int, ObjectType> OnBreakEvent;

    private void Awake()
    {
        SingletonPattern();
        InitialiseSpawnPoints();
    }

    private void SingletonPattern()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void InitialiseSpawnPoints()
    {
        spawnPoints.Clear();

        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
    }

    // In here subscribe to each individual object's "OnBreak" event with argument playerID and EnumObjectType objectType
    // Also create function that runs when an object is broken. Invoke "SomethingBroke" with same arguments.
    public void SpawnObjects()
    {
        foreach (Transform spawn in spawnPoints)
        {
            GameObject spawned = PhotonNetwork.InstantiateRoomObject(objectPrefab.name, spawn.position, spawn.rotation);
            BreakableObject bo = GetBreakableObj(spawned);
            bo.OnBreak += BreakEvent;
        }
    }

    private void BreakEvent(int playerID, ObjectType objectType)
    {
        // Invoke its own BreakEvent event action
        // When refactoring move Alert system call to listen to this event too
        OnBreakEvent?.Invoke(playerID, objectType);
    }

    public void DestroyForAll(GameObject obj)
    {
        PhotonNetwork.Destroy(obj);
        UnsubscribeOnBreak(obj);
    }

    private BreakableObject GetBreakableObj(GameObject obj)
    {
        if (obj.TryGetComponent<BreakableObject>(out BreakableObject bo))
        {
            return bo;
        }
        else
        {
            return null;
        }
    }

    private void UnsubscribeOnBreak(GameObject obj)
    {
        BreakableObject bo = GetBreakableObj(obj);
        bo.OnBreak -= BreakEvent;
    }

}
