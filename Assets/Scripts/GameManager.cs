using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    [Header("Players")]
    [SerializeField] private string playerPrefabPath;
    [SerializeField] private Transform[] spawnPoints;

    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    private int playersInGame;

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
        photonView.RPC("JoiningGame", RpcTarget.AllBuffered);
        SoundManager.instance.SubscribeToObjects();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
            }
        }
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

        if (playerScript == null) return;
        playerScript.photonView.RPC("Initialise", RpcTarget.All, PhotonNetwork.LocalPlayer);
        //ChaosManager.instance.AddPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
        photonView.RPC(nameof(AddChaosManagerPlayer), RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);

        SoundManager.instance?.SubscribeToPlayer(playerScript);
        }

    private void AddChaosManagerPlayer(int ID)
    {
        ChaosManager.instance.AddPlayer(ID);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        AlertManager.instance.ResubscribeEnemies();
    }

    // Add money stuff and data stored during session
    // Add rounds and Game Start.
}
