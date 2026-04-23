using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    [Header("Players")]
    [SerializeField] private string playerPrefabPath;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] public PlayerController[] players;
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
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("JoiningGame", RpcTarget.AllBuffered);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        ChaosManager.instance.photonView.RPC(nameof(ChaosManager.instance.InitChaos), RpcTarget.All, PhotonNetwork.PlayerList.Length);
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
        if (playerScript == null) return;
        playerScript.photonView.RPC("Initialise", RpcTarget.All, PhotonNetwork.LocalPlayer);
        //ChaosManager.instance.AddPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
        photonView.RPC(nameof(AddChaosManagerPlayer), RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);

        if (SoundManager.instance == null) return;
        SoundManager.instance.SubscribeToPlayer(playerScript);
        }

    private void AddChaosManagerPlayer(int ID)
    {
        ChaosManager.instance.AddPlayer(ID);
    }

    // Add money stuff and data stored during session
    // Add rounds and Game Start.
}
