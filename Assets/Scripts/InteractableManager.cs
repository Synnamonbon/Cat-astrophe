using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager instance;

    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private GameObject objectSpawner;
    [SerializeField] private GameObject foodSpawner;

    private List<Transform> objectSpawnPoints = new List<Transform>();
    private List<Transform> foodSpawnPoints = new List<Transform>();

    public event Action<int, ObjectType, Vector3, string> OnBreakEvent;
    public event Action<int, int> OnBreakOnNPCEvent;

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
        objectSpawnPoints.Clear();
        foodSpawnPoints.Clear();

        if (objectSpawner != null) {
            foreach (Transform child in objectSpawner.transform)
            {
                objectSpawnPoints.Add(child);
            }
        }
        if (foodSpawner != null){
            foreach (Transform child in foodSpawner.transform)
            {
                foodSpawnPoints.Add(child);
            }
        }
    }

    public void SpawnObjects()
    {
        if (objectSpawnPoints != null){
            foreach (Transform spawn in objectSpawnPoints)
            {
                GameObject spawned = PhotonNetwork.InstantiateRoomObject(objectPrefab.name,spawn.position, spawn.rotation);
                BreakableObject bo = GetBreakableObj(spawned);
                bo.OnBreak += BreakEvent;
                bo.OnBreakOnNPC += NPCBreakEvent;
            }
        }
        if (foodSpawnPoints != null){
            foreach (Transform spawn in foodSpawnPoints)
            {
                PhotonNetwork.InstantiateRoomObject(foodPrefab.name,spawn.position, spawn.rotation);
            }
        }
    }

    private void BreakEvent(int playerID, ObjectType objectType, Vector3 objectPosition, string tag)
    {
        // Invoke its own BreakEvent event action
        // When refactoring move Alert system call to listen to this event too
        OnBreakEvent?.Invoke(playerID, objectType, objectPosition, tag);         // Add tag
    }

    private void NPCBreakEvent(int playerID, int NPCID)
    {
        OnBreakOnNPCEvent?.Invoke(playerID, NPCID);
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

    public void DestroyForAll(GameObject obj)
    {
        PhotonView pv = obj.GetComponent<PhotonView>();
        if (pv == null) return;
        UnsubscribeOnBreak(obj);
        PhotonNetwork.Destroy(obj);
    }

}