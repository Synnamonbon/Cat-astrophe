using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager instance;

    [SerializeField] private GameObject objectSpawner;
    [SerializeField] private GameObject foodSpawner;
    [SerializeField] private GameObject draggableSpawner;

    [Header("Food spawn chance")]
    [SerializeField] private float smallFoodChance = 30f;
    [SerializeField] private float mediumFoodChance = 30f;
    [SerializeField] private float largeFoodChance = 30f;
    [SerializeField] private float rareFoodChance = 10f;

    private List<GameObject> breakables;
    private List<GameObject> smallFoods;
    private List<GameObject> mediumFoods;
    private List<GameObject> largeFoods;
    private List<GameObject> rareFoods;
    private List<GameObject> interactables;

    private List<Transform> objectSpawnPoints = new List<Transform>();
    private List<Transform> foodSpawnPoints = new List<Transform>();
    private List<Transform> interactableSpawnPoints = new List<Transform>();

    public event Action<int, ObjectType, Vector3, string> OnBreakEvent;
    public event Action<Transform, float> BreakAlert;
    public event Action<int, int> OnBreakOnNPCEvent;
    public event Action<int, string> OnDraggedFarEvent;

    private void Awake()
    {
        SingletonPattern();
        LoadAllObjectPrefabs();
        InitialiseSpawnPoints();
    }

    private void Start()
    {
        GetLevelInteractables();
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
        smallFoods = new List<GameObject>(Resources.LoadAll<GameObject>("Foods/Small"));
        mediumFoods = new List<GameObject>(Resources.LoadAll<GameObject>("Foods/Medium"));
        largeFoods = new List<GameObject>(Resources.LoadAll<GameObject>("Foods/Large"));
        rareFoods = new List<GameObject>(Resources.LoadAll<GameObject>("Foods/Rare"));
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

    public void SpawnObjects(int objects, int foods, int toys)
    {
        if (objectSpawnPoints != null && objects >= 0){
            SpawnBreakablesToSpawnpoints(objects);
        }
        if (foodSpawnPoints != null && foods >= 0){
            SpawnFoodsToSpawnpoints(foods);
        }
        if (interactableSpawnPoints != null && toys >= 0){
            SpawnInteractablesToSpawnpoints(toys);
        }
    }

    private void GetLevelInteractables()
    {
        foreach (GameObject painting in GameObject.FindGameObjectsWithTag("Painting"))
        {
            BreakableObject bo = GetBreakableObj(painting);
            bo.OnBreak += BreakEvent;
            bo.OnBreakOnNPC += NPCBreakEvent;
        }

        foreach (GameObject doc in GameObject.FindGameObjectsWithTag("Document"))
        {
            BreakableObject bo = GetBreakableObj(doc);
            bo.OnBreak += BreakEvent;
            bo.OnBreakOnNPC += NPCBreakEvent;
        }

        foreach (GameObject trash in GameObject.FindGameObjectsWithTag("Rubbish"))
        {
            ObjectDrag od = GetDraggableObject(trash);
            od.OnDraggedFar += DragFarEvent;
        }

         foreach (GameObject oit in GameObject.FindGameObjectsWithTag("Water Source"))
        {
            Drinkable od = GetDrinkableObject(oit);
            // Sub to VFX events here
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

                PhotonNetwork.InstantiateRoomObject(GetRandomFood(), spawns[j].position, spawns[j].rotation);

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

    private string GetRandomFood()
    {
        float totalFoodChance = smallFoodChance + mediumFoodChance + largeFoodChance + rareFoodChance;

        float roll = UnityEngine.Random.Range(0f, totalFoodChance);
        if (roll < smallFoodChance)
        {
            // Small food
            int idx = UnityEngine.Random.Range(0, smallFoods.Count);
            return "Foods/Small/" + smallFoods[idx].name;
        }
        else if (roll < smallFoodChance + mediumFoodChance)
        {
            // Medium food
            int idx = UnityEngine.Random.Range(0, mediumFoods.Count);
            return "Foods/Medium/" + mediumFoods[idx].name;
        }
        else if (roll < smallFoodChance + mediumFoodChance + largeFoodChance)
        {
            // Large food
            int idx = UnityEngine.Random.Range(0, largeFoods.Count);
            return "Foods/Large/" + largeFoods[idx].name;
        }
        else
        {
            // Rare food
            int idx = UnityEngine.Random.Range(0, rareFoods.Count);
            return "Foods/Rare/" + rareFoods[idx].name;
        }
    }

    private GameObject GetRandomInteractable()
    {
        int idx = UnityEngine.Random.Range(0, interactables.Count);
        return interactables[idx];
    }

    private void BreakEvent(GameObject go, ObjectType objectType, Vector3 objectPosition, string tag, float dist)
    {
        BreakAlert?.Invoke(go.transform, dist);
        OnBreakEvent?.Invoke(go.GetPhotonView().Owner.ActorNumber, objectType, objectPosition, tag);
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

    private Drinkable GetDrinkableObject(GameObject obj)
    {
        if (obj.TryGetComponent<Drinkable>(out Drinkable od))
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