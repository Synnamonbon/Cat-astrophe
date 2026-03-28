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

        foreach (Transform child in objectSpawner.transform)
        {
            objectSpawnPoints.Add(child);
        }
        foreach (Transform child in foodSpawner.transform)
        {
            foodSpawnPoints.Add(child);
        }
    }

    public void SpawnObjects()
    {
        foreach (Transform spawn in objectSpawnPoints)
        {
            PhotonNetwork.InstantiateRoomObject(objectPrefab.name,spawn.position, spawn.rotation);
        }
        foreach (Transform spawn in foodSpawnPoints)
        {
            PhotonNetwork.InstantiateRoomObject(foodPrefab.name,spawn.position, spawn.rotation);
            Debug.Log($"{spawn.name} | self: {spawn.gameObject.activeSelf} | hierarchy: {spawn.gameObject.activeInHierarchy}");
        }
    }

    public void DestroyForAll(GameObject obj)
    {
        PhotonNetwork.Destroy(obj);
    }

}