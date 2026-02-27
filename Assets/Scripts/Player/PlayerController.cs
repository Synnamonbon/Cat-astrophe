using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Player photonPlayer;
    [HideInInspector] public int id;

    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;

    private Rigidbody playerRB;

    private void Awake()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerMovement = gameObject.GetComponent<PlayerMovement>();
        playerAttack = gameObject.GetComponent<PlayerAttack>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            playerMovement.enabled = true;
            playerAttack.enabled = true;
        }
    }

    [PunRPC]
    public void Initialise (Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;
        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine)
            playerRB.isKinematic = true;
    }
}
