using Photon.Pun;
using Photon.Realtime;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [HideInInspector] public Player photonPlayer;
    [SerializeField] private BoxCollider playerGroundingBC;
    [HideInInspector] public int id;

    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private PlayerGrounding playerGrounding;

    private Rigidbody playerRB;

    private void Awake()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerMovement = gameObject.GetComponent<PlayerMovement>();
        playerAttack = gameObject.GetComponent<PlayerAttack>();
        playerGrounding = gameObject.GetComponentInChildren<PlayerGrounding>();

        if (photonView.IsMine)
        {
            playerMovement.enabled = true;
            playerAttack.enabled = true;
            playerGroundingBC.enabled = true;
            playerGrounding.enabled = true;
        }
    }

    private void Start()
    {
        // Make camera follow player
        if (!photonView.IsMine) return;
        CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();
        //Debug.Log(cam);
        cam.Follow = transform;
    }

    [PunRPC]
    public void Initialise (Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;
        if (TestSceneStart.instance != null) TestSceneStart.instance.player = this;

        if (!photonView.IsMine){
            playerRB.isKinematic = true;
            playerRB.useGravity = false;
        }
    }
}
