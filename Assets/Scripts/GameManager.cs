using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    [Header("Players")]
    [SerializeField] private string playerPrefabPath;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float gameLength = 240f;       // In Seconds
    [SerializeField] private float spawnNewObjectsTimer = 120f;
    [SerializeField] private int OBJECTS_PER_PLAYER;
    [SerializeField] private int FOODS_PER_PLAYER;
    [SerializeField] private int TOYS_PER_PLAYER;
    [SerializeField] private int FOODS_RESPAWN_PER_PLAYER;

    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    private int playersInGame;
    public event Action<float> SetTimer;
    public event Action EndGame;
    public event Action<int, string> InteractForTask;
    public event Action<int, string> PawForTask;
    public event Action<int, string> MeowForTask;
    public event Action<Transform, float> SoundAlertForNPC;
    private float timeRemaining;
    private float lastTimeSpawned;

    private void Awake()
    {
        SingletonPattern();
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

    private void Start()
    {
        instance.photonView.RPC(nameof(JoiningGame), RpcTarget.AllBuffered);
        SoundManager.instance.SubscribeToObjects();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // ChaosManager.instance.photonView.RPC(nameof(ChaosManager.instance.InitChaos), RpcTarget.All, PhotonNetwork.PlayerList.Length);
        ChaosManager.instance.GameWonEvent += EndRoundProcedure;
    }

    private void FixedUpdate()
    {
        timeRemaining -= Time.fixedDeltaTime;
        if (timeRemaining <= 0f)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            EndGame?.Invoke();
        }
        if (Time.time - lastTimeSpawned >= spawnNewObjectsTimer)
        {
            lastTimeSpawned = Time.time;
            if (PhotonNetwork.IsMasterClient)
            {
                InteractableManager.instance.SpawnObjects(-1, PhotonNetwork.PlayerList.Length * FOODS_RESPAWN_PER_PLAYER, -1);
            }
        }
    }

    private void OnDestroy()
    {
        SoundManager.instance.UnSubscribeToObjects();
    }

    [PunRPC]
    public void JoiningGame()
    {
        playersInGame ++;

        if(playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
            if (PhotonNetwork.IsMasterClient)
            {
                int n = PhotonNetwork.PlayerList.Length;
                InteractableManager.instance.SpawnObjects(n * OBJECTS_PER_PLAYER, n * FOODS_PER_PLAYER, n * TOYS_PER_PLAYER);
                NPCManager.instance.SpawnEnemies();
                ChaosManager.instance.photonView.RPC(nameof(ChaosManager.instance.InitChaos), RpcTarget.AllBuffered, PhotonNetwork.PlayerList.Length);
                // To ensure round starts only once, start the game timer here?
                instance.photonView.RPC(nameof(GameStart), RpcTarget.AllBuffered);
            }
        }
    }

    private void SpawnPlayer()
    {
        GameObject playerObject = PhotonNetwork.Instantiate(
            playerPrefabPath, 
            spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position, 
            Quaternion.identity);

        PlayerController playerScript = playerObject.GetComponent<PlayerController>();
        SubToPlayerEvents(playerScript);

        // Track every player's controller
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        players[actorNumber] = playerScript;

        if (playerScript == null) return;
        playerScript.photonView.RPC("Initialise", RpcTarget.All, PhotonNetwork.LocalPlayer);
        //ChaosManager.instance.AddPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
        instance.photonView.RPC(nameof(AddChaosManagerPlayer), RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);

        SoundManager.instance?.SubscribeToPlayer(playerScript);
    }

    [PunRPC]
    private void AddChaosManagerPlayer(int ID)
    {
        ChaosManager.instance.AddPlayer(ID);
    }

    private void SubToPlayerEvents(PlayerController pc)
    {
        pc.InteractEventDelegate += InteractDelegateEvent;
        pc.PawEventDelegate += PawDelegateEvent;
        pc.MeowEventDelegate += MeowDelegateEvent;
        pc.SoundAlertDelegate += SoundDelegateAlert;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        AlertManager.instance.ResubscribeEnemies();
    }

    private void InteractDelegateEvent(int playerID, string tag)
    {
        InteractForTask?.Invoke(playerID, tag);
    }

    private void PawDelegateEvent(int playerID, string tag)
    {
        PawForTask?.Invoke(playerID, tag);
    }

    private void MeowDelegateEvent(int playerID, string layer)
    {
        MeowForTask?.Invoke(playerID, layer);
    }

    private void SoundDelegateAlert(Transform tf, float radius)
    {
        SoundAlertForNPC?.Invoke(tf, radius);
    }

    // Add money stuff and data stored during session
    // Add rounds and Game Start.
    [PunRPC]
    private void GameStart()
    {
        instance.timeRemaining = instance.gameLength;
        instance.lastTimeSpawned = Time.time;
        SetTimer?.Invoke(instance.timeRemaining);
    }

    private void EndRoundProcedure(bool win)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (win)
            {
                Debug.Log("You won! Load shop area");

            }
            else
            {
                Debug.Log("You lose! Start again");
            }
        }
        SoundManager.instance.UnSubscribeToObjects();
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "GameOver");
    }
}
