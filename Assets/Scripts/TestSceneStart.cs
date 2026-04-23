using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using Unity.Cinemachine;
using UnityEngine;

public class TestSceneStart : MonoBehaviourPunCallbacks
{
    public static TestSceneStart instance;
    [Header("Players")]
    [SerializeField] private string playerPrefabPath;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] public PlayerController player;

    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        SoundManager.instance.SubscribeToObjects();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        SoundManager.instance.UnSubscribeToObjects();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to room");
        StartCoroutine(ConnectToRoom());
    }

    private IEnumerator ConnectToRoom()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        GameObject playerObject = PhotonNetwork.Instantiate(
            playerPrefabPath, 
            spawnPoints[Random.Range(0, spawnPoints.Length)].position, 
            Quaternion.identity);

        PlayerController playerScript = playerObject.GetComponent<PlayerController>();

        // Track every player's controller
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        players[actorNumber] = playerScript;

        if (PhotonNetwork.IsMasterClient)
        {
            InteractableManager.instance.SpawnObjects();
            NPCManager.instance.SpawnEnemies();
        }
        else
        {
            AlertManager.instance.photonView.RPC(nameof(AlertManager.instance.ResubscribeEnemies), RpcTarget.All);
        }

        if (playerScript == null) return;
        playerScript.photonView.RPC("Initialise", RpcTarget.All, PhotonNetwork.LocalPlayer);

        if (SoundManager.instance == null) return;
        SoundManager.instance.SubscribeToPlayer(playerScript);
    }

    private void AddChaosManagerPlayer(int ID)
    {
        ChaosManager.instance.AddPlayer(ID);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        AlertManager.instance.ResubscribeEnemies();
    }

}
