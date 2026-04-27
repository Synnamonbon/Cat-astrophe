using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager instance;

    [SerializeField] private GameObject objectSpawner;
    [SerializeField] private GameObject foodSpawner;
    [SerializeField] private GameObject draggableSpawner;
    private List<GameObject> breakables;
    private List<GameObject> foods;
    private List<GameObject> interactables;

    private List<Transform> objectSpawnPoints = new List<Transform>();
    private List<Transform> foodSpawnPoints = new List<Transform>();
    private List<Transform> interactableSpawnPoints = new List<Transform>();

    public event Action<int, ObjectType, Vector3, string> OnBreakEvent;
    public event Action<int, int> OnBreakOnNPCEvent;
    public event Action<int, string> OnDraggedFarEvent;

    private void Awake()
    {
        SingletonPattern();
        LoadAllObjectPrefabs();
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

    private void LoadAllObjectPrefabs()
    {
        breakables = new List<GameObject>(Resources.LoadAll<GameObject>("Breakables"));
        foods = new List<GameObject>(Resources.LoadAll<GameObject>("Foods"));
        interactables = new List<GameObject>(Resources.LoadAll<GameObject>("Interactables"));
    }

    private void InitialiseSpawnPoints()
    {
        objectSpawnPoints.Clear();
        foodSpawnPoints.Clear();
        interactableSpawnPoints.Clear();

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
        if (draggableSpawner != null){
            foreach (Transform child in draggableSpawner.transform)
            {
                interactableSpawnPoints.Add(child);
            }
        }
    }

    public void SpawnObjects()
    {
        if (objectSpawnPoints != null){
            SpawnBreakablesToSpawnpoints(5);
        }
        if (foodSpawnPoints != null){
            SpawnFoodsToSpawnpoints(3);
        }
        if (interactableSpawnPoints != null){
            SpawnInteractablesToSpawnpoints(7);
        }
    }

    // If number to spawn is 0, spawn on each spawn point instead
    private void SpawnBreakablesToSpawnpoints(int numberToSpawn)
    {
        if (objectSpawnPoints != null){
            if (numberToSpawn > objectSpawnPoints.Count || numberToSpawn == 0)
            {
                numberToSpawn = objectSpawnPoints.Count;
            }
            List<Transform> spawns = new(objectSpawnPoints);

            for (int i = 0; i < numberToSpawn; i++)
            {
                int j = UnityEngine.Random.Range(0, spawns.Count);

                GameObject spawned = PhotonNetwork.InstantiateRoomObject("Breakables/" + GetRandomBreakable().name, spawns[j].position, spawns[j].rotation);
                BreakableObject bo = GetBreakableObj(spawned);
                bo.OnBreak += BreakEvent;
                bo.OnBreakOnNPC += NPCBreakEvent;

                spawns.RemoveAt(j);
            }
        }
    }

    private void SpawnFoodsToSpawnpoints(int numberToSpawn)
    {
        if (foodSpawnPoints != null){
            if (numberToSpawn > foodSpawnPoints.Count || numberToSpawn == 0)
            {
                numberToSpawn = foodSpawnPoints.Count;
            }
            List<Transform> spawns = new(foodSpawnPoints);

            for (int i = 0; i < numberToSpawn; i++)
            {
                int j = UnityEngine.Random.Range(0, spawns.Count);

                PhotonNetwork.InstantiateRoomObject("Foods/" + GetRandomFood().name, spawns[j].position, spawns[j].rotation);

                spawns.RemoveAt(j);
            }
        }
    }

    private void SpawnInteractablesToSpawnpoints(int numberToSpawn)
    {
        if (interactableSpawnPoints != null){
            if (numberToSpawn > interactableSpawnPoints.Count || numberToSpawn == 0)
            {
                numberToSpawn = interactableSpawnPoints.Count;
            }
            List<Transform> spawns = new(interactableSpawnPoints);

            for (int i = 0; i < numberToSpawn; i++)
            {
                int j = UnityEngine.Random.Range(0, spawns.Count);

                GameObject spawned = PhotonNetwork.InstantiateRoomObject("Interactables/" + GetRandomInteractable().name, spawns[j].position, spawns[j].rotation);
                ObjectDrag od = GetDraggableObject(spawned);
                od.OnDraggedFar += DragFarEvent;

                spawns.RemoveAt(j);
            }
        }
    }

    private GameObject GetRandomBreakable()
    {
        int idx = UnityEngine.Random.Range(0, breakables.Count);
        return breakables[idx];
    }

    private GameObject GetRandomFood()
    {
        int idx = UnityEngine.Random.Range(0, foods.Count);
        return foods[idx];
    }

    private GameObject GetRandomInteractable()
    {
        int idx = UnityEngine.Random.Range(0, interactables.Count);
        return interactables[idx];
    }

    private void BreakEvent(int playerID, ObjectType objectType, Vector3 objectPosition, string tag)
    {
        // Invoke its own BreakEvent event action
        // When refactoring move Alert system call to listen to this event too
        OnBreakEvent?.Invoke(playerID, objectType, objectPosition, tag);
    }

    private void NPCBreakEvent(int playerID, int NPCID)
    {
        OnBreakOnNPCEvent?.Invoke(playerID, NPCID);
    }

    private void DragFarEvent(int playerID, string tag)
    {
        OnDraggedFarEvent?.Invoke(playerID, tag);
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

    private ObjectDrag GetDraggableObject(GameObject obj)
    {
        if (obj.TryGetComponent<ObjectDrag>(out ObjectDrag od))
        {
            return od;
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