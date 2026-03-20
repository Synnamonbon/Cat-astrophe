using System.Collections;
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

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        playerScript.photonView.RPC("Initialise", RpcTarget.All, PhotonNetwork.LocalPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            ObjectManager.instance.SpawnObjects();
            NPCManager.instance.SpawnEnemies();
        }
        else
        {
            AlertManager.instance.photonView.RPC(nameof(AlertManager.instance.ResubscribeEnemies), RpcTarget.All);
            //AlertManager.instance.SubscribeEnemiesToPlayer(playerObject);
        }
    }

}
