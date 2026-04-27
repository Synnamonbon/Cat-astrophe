using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    [Header("Players")]
    [SerializeField] private string playerPrefabPath;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float gameLength = 240f;       // In Seconds
    [SerializeField] private float spawnNewObjectsTimer = 120f;

    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    private int playersInGame;
    public event Action<float> SetTimer;
    public event Action EndGame;
    public event Action<int, string> InteractForTask;
    public event Action<int, string> PawForTask;
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
            EndGame?.Invoke();
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
                InteractableManager.instance.SpawnObjects();
                ChaosManager.instance.photonView.RPC(nameof(ChaosManager.instance.InitChaos), RpcTarget.All, PhotonNetwork.PlayerList.Length);
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
    }
}
