using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;

public class EnemyNPCDetection : MonoBehaviourPunCallbacks
{
    public GameObject visionOrigin;
    public GameObject visionRot;
    private List<GameObject> playersList = new List<GameObject>();
    private GameObject playerInChase = null;
    private float chaseTargetLastSeen = 0f;
    private bool isCapturing = false;
    private float captureStartTime;
    private bool isCarrying = false;
    private bool isBlinded = false;
    [Header("Detection tinkering:")]
    [Range(0f, 100f)]
    [SerializeField] private float visionDistance = 10f;
    [Range(5f, 100f)]
    [SerializeField] private float visionAngle = 80f;
    [Range(0.1f, 3f)]
    [SerializeField] private float chaseFollowthrough = 1f;
    [SerializeField] private float capturingReach = 1f;
    [SerializeField] private float capturingDuration = 1f;

    private EnemyNPCNavigation enemyNav;

    private void OnEnable()
    {
        playersList = new List<GameObject>();
    }

    private void Start()
    {
        playersList = GameObject.FindGameObjectsWithTag("Player").ToList();
        
        if (gameObject.TryGetComponent<EnemyNPCNavigation>(out EnemyNPCNavigation nav))
        {
            enemyNav = nav;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) {return;}
        if (isCarrying) {return;}
        if (isBlinded) {return;}
        DetectPlayers();
        CapturingDetect();
    }

    public void ResubscribeEnemy()
    {
        playersList = new List<GameObject>();
        playersList = GameObject.FindGameObjectsWithTag("Player").ToList();
    }

    private GameObject[] DetectPlayers()
    {
        GameObject nearestCat = null;
        float nearestDist = visionDistance;
        // We want to only detect the nearest player
        foreach (GameObject player in playersList)
        {
            if (player == null) break;  
            Vector3 lookTarget = new Vector3(player.transform.position.x,
                                            player.transform.position.y,
                                            player.transform.position.z);
            Vector3 dir = lookTarget - visionOrigin.transform.position;
            RaycastHit hit;
            if (Physics.Raycast(visionOrigin.transform.position, dir, out hit, visionDistance))         // Distance handling
            {
                Debug.DrawRay(visionOrigin.transform.position, dir.normalized * hit.distance, Color.yellow);
                GameObject hitObj = hit.transform.gameObject;
                
                float theta = Vector3.Angle(visionOrigin.transform.forward, dir);                       // Angle checking
                //Debug.Log(theta + " Degrees for " + player.name);
                if (theta > visionAngle){continue;}
                
                if (!hitObj.CompareTag("Player")){continue;}                                            // Tag checking

                if (hitObj.TryGetComponent<PlayerController>(out PlayerController pc))                  // Check if the cat is visible
                {
                    if (!pc.isVisible) {continue;}
                }
                
                // We are seeing a cat, check if its the nearest one
                Debug.DrawRay(visionOrigin.transform.position, dir.normalized * hit.distance, Color.yellow);
                if (nearestDist >= hit.distance)
                {
                    nearestDist = hit.distance;
                    nearestCat = hitObj;
                }
            }
        }

        // Call See cat on the nearest cat
        if (nearestCat != null)
        {
            SeeCat(nearestCat);
        }   // If we arent seeing a cat but we are in chase and we have seen the cat chasePatience seconds ago.
        else if (playerInChase != null && chaseTargetLastSeen + chaseFollowthrough > Time.time)
        {
            // Update Chase Target
            enemyNav.UpdateChasingTarget(playerInChase.transform.position);
            // Maybe also reset visionOrigin to forward
        }

        return null;
    }

    private void SeeCat(GameObject player)
    {
        // Debug.Log("Seeing " + player.name);
        // Begin chase with cat if we arent yet
        if (playerInChase == null)
        {
            playerInChase = player;
        }   // If we are seeing a new cat who is closer we should swap chase over
        else if (playerInChase != player)
        {
            playerInChase = player;
        }
        // Send Chase Cat command to enemy nav system, which will switch it into chase mode.
        enemyNav.UpdateChasingTarget(playerInChase.transform.position);
        chaseTargetLastSeen = Time.time;

        // Maybe also make the visionOrigin.forward face towards the player?
    }

    public void EndChase()
    {
        // Call this from Nav system if we have reached the destination and have not captured the cat to tell the detection system to reset.
        playerInChase = null;
        isCapturing = false;
        // Maybe also reset visionOrigin
    }

    private void CapturingDetect()
    {
        if (playerInChase != null)
        {
            float dist = GetChaseDist();
            // Debug.Log(dist + " units away!");
            if (dist <= capturingReach)     // In Capturing Range
            {
                if (!isCapturing)           // if we are just now starting to capture, set start time as now
                {
                    captureStartTime = Time.time;
                }
                isCapturing = true;

                if (Time.time >= captureStartTime + capturingDuration)
                {
                    // Capture cat
                    int caughtID = playerInChase.GetPhotonView().ViewID;
                    enemyNav.photonView.RPC(nameof(enemyNav.CaptureCat), RpcTarget.All, caughtID);
                }
            }
            else                            // Not In Capturing Range, simply reset
            {
                isCapturing = false;        // This effectively resets the capturing time as well
            }
        }
    }

    private float GetChaseDist()
    {
        return Vector3.Distance(new Vector3(playerInChase.transform.position.x, 0, playerInChase.transform.position.z), new Vector3(transform.position.x, 0, transform.position.z));
    }

    public void SetCarrying(bool set)
    {
        isCarrying = set;
    }

    public void BlindAgentForDuration(float waitTime)
    {
        StartCoroutine(BlindCoroutine(waitTime));
    }

    public IEnumerator BlindCoroutine(float waitTime)
    {
        Debug.Log("Blinded");
        isBlinded = true;
        yield return new WaitForSeconds(waitTime);
        isBlinded = false;
        Debug.Log("Unblinded");
    }
}
