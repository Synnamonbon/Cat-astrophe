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

    public void SpawnObjects()
    {
        foreach (Transform spawn in spawnPoints)
        {
            PhotonNetwork.InstantiateRoomObject(objectPrefab.name,spawn.position, spawn.rotation);
        }
    }

    public void DestroyForAll(GameObject obj)
    {
        PhotonNetwork.Destroy(obj);
    }

}
