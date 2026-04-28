using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NPCManager : MonoBehaviour
{
    public static NPCManager instance;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float stunDuration = 2.5f;

    public event Action<int> OnSaveCatEvent;

    private List<Transform> spawnPoints = new List<Transform>();

    private void Awake()
    {
        SingletonPattern();
        InitialiseSpawnPoints();
    }

    private void Start()
    {
        SubscribeToEvents();
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

    private void SubscribeToEvents()
    {
        InteractableManager.instance.OnBreakOnNPCEvent += HitNPCWithObject;
    }

    public void SpawnEnemies()
    {
        foreach (Transform spawn in spawnPoints)
        {
            GameObject spawned = PhotonNetwork.InstantiateRoomObject(enemyPrefab.name, spawn.position, spawn.rotation);
            AlertManager.instance.AddEnemy(spawned);
            if (spawned.TryGetComponent<EnemyNPCNavigation>(out EnemyNPCNavigation nav))
            {
                nav.SaveCat += OnSaveCat;
            }
        }
    }

    public void DestroyForAll(GameObject enemy)
    {
        PhotonNetwork.Destroy(enemy);
        AlertManager.instance.RemoveEnemy(enemy);
    }

    private void HitNPCWithObject(int playerID, int NPCID)
    {
        // Get selected NPC gameobject
        EnemyNPCNavigation npc = GetNPC(NPCID);
        if (npc != null)
        {
            // Stun NPC for set duration
            // Disable vision for same duration inside the enemy nav perhaps
            npc.StunForDuration(stunDuration, playerID);
        }
    }

    private void OnSaveCat(int playerID)
    {
        OnSaveCatEvent?.Invoke(playerID);
    }

    private EnemyNPCNavigation GetNPC(int NPCID)
    {
        GameObject go = PhotonView.Find(NPCID).gameObject;
        if (go.TryGetComponent<EnemyNPCNavigation>(out EnemyNPCNavigation nav))
        {
            return nav;
        }
        else
        {
            return null;
        }
    }
}
