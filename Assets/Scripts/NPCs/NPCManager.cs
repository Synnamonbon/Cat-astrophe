using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager instance;
    [SerializeField] private GameObject enemyPrefab;

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

    public void SpawnEnemies()
    {
        foreach (Transform spawn in spawnPoints)
        {
            GameObject spawned = PhotonNetwork.InstantiateRoomObject(enemyPrefab.name,spawn.position, spawn.rotation);
            AlertManager.instance.AddEnemy(spawned);
        }
    }

    public void DestroyForAll(GameObject enemy)
    {
        PhotonNetwork.Destroy(enemy);
        AlertManager.instance.RemoveEnemy(enemy);
    }
}
