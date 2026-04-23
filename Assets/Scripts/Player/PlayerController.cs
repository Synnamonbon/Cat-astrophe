using System;
using Photon.Pun;
using Photon.Realtime;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [HideInInspector] public Player photonPlayer;
    [SerializeField] private BoxCollider playerGroundingBC;
    [SerializeField] private GameObject cameraPOV;
    [HideInInspector] public int id;
    [HideInInspector] public bool isVisible = true;         // Flag to check if the detection system should consider the cat as detecable

    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private PlayerGrounding playerGrounding;
    private PlayerHunger playerHunger;
    private PlayerMeow playerMeow;
    
    public event Action<Vector3> PlayerMeow;

    private Rigidbody playerRB;

    private void Awake()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerMovement = gameObject.GetComponent<PlayerMovement>();
        playerAttack = gameObject.GetComponent<PlayerAttack>();
        playerGrounding = gameObject.GetComponentInChildren<PlayerGrounding>();
        playerHunger = gameObject.GetComponent<PlayerHunger>();
        playerMeow = gameObject.GetComponent<PlayerMeow>();

        if (photonView.IsMine)
        {
            playerMovement.enabled = true;
            playerAttack.enabled = true;
            playerGroundingBC.enabled = true;
            playerGrounding.enabled = true;
            playerHunger.enabled = true;
            playerMeow.enabled = true;
        }
    }

    private void Start()
    {
        // Make camera follow player
        if (!photonView.IsMine) return;
        CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();
        UIManager playerUI = FindFirstObjectByType<UIManager>();
        //Debug.Log(cam);
        cam.Target.TrackingTarget = cameraPOV.transform;
        cam.PreviousStateIsValid = false;
        playerUI.AssignPlayerHungerUI(playerHunger);
    }

    private void OnDestroy()
    {
        PlayerMeow = null;
    }

    [PunRPC]
    public void Initialise (Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;
        if (TestSceneStart.instance != null) TestSceneStart.instance.player = this;

        if (!photonView.IsMine){
            playerRB.isKinematic = true;
            //playerRB.useGravity = false;
        }
    }

    public void MakePlayerMeow()
    {
        PlayerMeow?.Invoke(gameObject.transform.position);
    }
}
