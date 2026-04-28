using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using Unity.Cinemachine;
using UnityEngine;


public class PlayerController : MonoBehaviourPun
{
    private CinemachineCamera cinemachineCamera;
    [HideInInspector] public Player photonPlayer;
    [SerializeField] private BoxCollider playerGroundingBC;
    [SerializeField] private GameObject cameraPOV;
    [HideInInspector] public int id;
    [HideInInspector] public bool isVisible = true;        // Flag to check if the detection system should consider the cat as detecable
    private bool hitRecently = false;

    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private PlayerGrounding playerGrounding;
    private PlayerHunger playerHunger;
    private PlayerMeow playerMeow;
    private PlayerInteract playerInteract;
    
    public event Action<Vector3> PlayerMeow;
    public event Action<int, string> InteractEventDelegate;
    public event Action<int, string> PawEventDelegate;
    public event Action<int, string> MeowEventDelegate;

    private Rigidbody playerRB;

    private void Awake()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerMovement = gameObject.GetComponent<PlayerMovement>();
        playerAttack = gameObject.GetComponent<PlayerAttack>();
        playerGrounding = gameObject.GetComponentInChildren<PlayerGrounding>();
        playerHunger = gameObject.GetComponent<PlayerHunger>();
        playerMeow = gameObject.GetComponent<PlayerMeow>();
        playerInteract = gameObject.GetComponent<PlayerInteract>();

        if (photonView.IsMine)
        {
            playerMovement.enabled = true;
            playerAttack.enabled = true;
            playerGroundingBC.enabled = true;
            playerGrounding.enabled = true;
            playerHunger.enabled = true;
            playerMeow.enabled = true;
            playerInteract.enabled = true;
        }
        SubscribeToInteractEvents();
    }

    private void SubscribeToInteractEvents()
    {
        playerInteract.InteractWith += InteractEventTrigger;
        playerAttack.PawedAt += PawEventTrigger;
        playerMeow.OnMeowAndDistance += MeowEventTrigger;
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
        playerInteract.SetLookDir(cam.gameObject.transform);
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

    [PunRPC]
    public void TryGetHit(float forceMagnitude, Vector3 forceLocation)
    {
        float hitrange = 4f;
        float sqrDistance = (playerRB.transform.position - forceLocation).sqrMagnitude;

        if (sqrDistance > hitrange || hitRecently) return;
        StartCoroutine(JustGotHit());
        MakePlayerMeow();

        Vector3 hitDirection = (playerRB.transform.position - forceLocation).normalized;
        Debug.Log("Knocking back player");
        StartCoroutine(playerMovement.GetKnockedBack(forceMagnitude, hitDirection));
    }

    private IEnumerator JustGotHit()
    {
        hitRecently = true;
        yield return new WaitForSeconds(1f);
        hitRecently = false;
    }

    private void InteractEventTrigger(int playerID, string tag)
    {
        InteractEventDelegate?.Invoke(playerID, tag);
    }

    private void PawEventTrigger(int playerID, string tag)
    {
        PawEventDelegate?.Invoke(playerID, tag);
    }

    private void MeowEventTrigger(string layer)
    {
        MeowEventDelegate?.Invoke(PhotonNetwork.LocalPlayer.ActorNumber, layer);
    }
}
