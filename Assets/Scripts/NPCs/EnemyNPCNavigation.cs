using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Unity.Mathematics;

public class EnemyNPCNavigation : MonoBehaviourPunCallbacks
{
    [NonSerialized] public NavMeshAgent agent;
    private EnemyNPCDetection enemyDet;
    [NonSerialized] private List<Vector3> patrolRoute = new List<Vector3>();                // Patrol route
    public GameObject routeHolder;                                                          // Holder for patrol route points
    public CageScript[] cages;                                                              // Holds all cage dropoff points
    public GameObject catCarryPoint;
    private int currentWaypoint = 0;

    [Header("Patrol Balancing")]
    [SerializeField] private float destinationLeniency = 0.5f;
    [SerializeField] private float surveyDuration = 2f;
    [SerializeField] private float patrolSpeed = 4f;
    [Header("Alerted Balancing")]
    [SerializeField] private float alertSpeed = 6.5f;
    [Header("Chasing Balancing")]
    [SerializeField] private float chaseSpeed = 6.5f;
    [SerializeField] private float aggroDuration = 2f;
    [Header("Carrying Balancing")]
    [SerializeField] private float carrySpeed = 3f;

    private bool isLooking = false;
    private NPCStates currentState;
    private List<Vector3> alertSources = new List<Vector3>();
    private bool isAlertable = true;              // To be used to determine if NPC can be distracted right now. Turn off when chasing/carrying or if they are idling/lazing around?
    private Vector3 chaseTarget;
    private float chaseLastUpdated = 0f;
    private int nearestCageIDX = -1;
    private PlayerController carriedCat;

    private void OnEnable()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = destinationLeniency/2;

        enemyDet = gameObject.GetComponent<EnemyNPCDetection>();

        GameObject[] routes = GameObject.FindGameObjectsWithTag("Route");
        // Currently only gets the first instance of a route holder
        routeHolder = routes[0];

        foreach(Transform child in routeHolder.GetComponentsInChildren<Transform>())
        {
            patrolRoute.Add(child.position);
        }
        patrolRoute.Remove(routeHolder.GetComponent<Transform>().position);

        // List of spawnpoints inside of cages.
        GameObject[] cagesGOs = GameObject.FindGameObjectsWithTag("CagePoint");
        cages = new CageScript[cagesGOs.Length];
        for(int i = 0; i < cagesGOs.Length; i++)
        {
            if (cagesGOs[i].TryGetComponent<CageScript>(out CageScript cs))
            {
                cages[i] = cs;
            }
        }

        currentState = NPCStates.Patrolling;

        if (!PhotonNetwork.IsMasterClient)
        {
            agent.enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (agent.enabled == false) return;

        switch (currentState)
        {
            case NPCStates.Patrolling:
                agent.speed = patrolSpeed;
                Patrol();
                break;
            case NPCStates.Alerted:
                agent.speed = alertSpeed;
                GoToSoundSource();
                break;
            case NPCStates.Surveying:
                // we want to start looking around only once. When that is done we want to return to the previous state, Alerted or Patrolling.
                if (!isLooking)
                {
                    ChangeState();
                }
                break;
            case NPCStates.Chasing:
                agent.speed = chaseSpeed;
                GoTowardsChaseTarget();
                break;
            case NPCStates.Carrying:
                agent.speed = carrySpeed;
                CarryCat();
                break;
            default:
                Debug.Log("Unhandled State for " + gameObject.name);
                break;
        }
    }

    private void Patrol()
    {
        if(patrolRoute.Count == 0)
        {
           // Debug.Log("Where's my patrol path :<");
            return;
        }

        float dist = GetDistance(patrolRoute[currentWaypoint], transform.position);
        SurveyIfArrived(dist);
        
        agent.SetDestination(patrolRoute[currentWaypoint]);
    }

    private void IncrementPatrolPoint()
    {
        currentWaypoint = (currentWaypoint + 1) % patrolRoute.Count;
    }

    private void ChangeState()
    {
        if (nearestCageIDX != -1)
        {
            currentState = NPCStates.Carrying;
        }
        else if (alertSources.Count == 0)
        {
            currentState = NPCStates.Patrolling;
        }
        else
        {
            currentState = NPCStates.Alerted;
        }
    }

    private IEnumerator LookAround()
    {
        Debug.Log("Looking");
        isLooking = true;
        agent.isStopped = true;

        // Surveying behaviour.
        float startRotation = transform.eulerAngles.y;
        float endRotation = startRotation + 360.0f;
        float t = 0.0f;
        while (t  < surveyDuration)
        {
            if (currentState == NPCStates.Chasing)
            {
                Debug.Log("STOP LOOKING, I SEE CAT!");
                isLooking = false;
                agent.isStopped = false;
                yield break;
            }
            t += Time.deltaTime;
            float yRotation = Mathf.Lerp(startRotation, endRotation, t / 2.0f) % 360.0f;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
            yield return null;
        }
        isLooking = false;
        agent.isStopped = false;
    }

    [PunRPC]
    public void AlertToSound(Vector3 source)
    {
        // Behaviour to being alerted to a sound.
        // Currently just stop the current path and go to the source of the object.
        // If new Alert comes, empty list and set newest source at low chaos
        // At high chaos, add to end of list but keep pursuing first.
        if (!PhotonNetwork.IsMasterClient) {return;}
        
        if (alertSources.Count > 0)
        {
            alertSources = new List<Vector3>();
        }

        alertSources.Add(source);
        
        if (isAlertable && currentState != NPCStates.Alerted && currentState != NPCStates.Surveying)
        {
            StartCoroutine(StopAgentForDuration(1f));
            currentState = NPCStates.Alerted;
        }
    }

    private void GoToSoundSource()
    {
        if (alertSources.Count == 0)
        {
            // We shouldnt be able to go in here but futureproofing if NPCs can tell others they checked stuff out already.
            Debug.Log("No sources to go to. Return to patrolling");
            currentState = NPCStates.Patrolling;
            return;
        }

        float dist = GetDistance(alertSources[0], transform.position);
        SurveyIfArrived(dist);

        // We check again because SurveyIfArrived could have removed our Alert Source
        if (alertSources.Count != 0)
        {
            agent.SetDestination(alertSources[0]);
        }
    }

    private float GetDistance(Vector3 goal, Vector3 source)
    {
        return Vector3.Distance(goal, source);
    }

    private void SurveyIfArrived(float dist)
    {
        if (dist < destinationLeniency)
        {
            if (currentState == NPCStates.Alerted)
            {
                alertSources.RemoveAt(0);
            }
            else if (currentState == NPCStates.Patrolling)
            {
                IncrementPatrolPoint();
            }

            currentState = NPCStates.Surveying;
            StartCoroutine(LookAround());
        }
    }

    public IEnumerator StopAgentForDuration(float waitTime)
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(waitTime);
        agent.isStopped = false;
    }

    // Signal that the detection system has seen a cat
    // Set state to chasing to immediately start chasing, block out alerts to focus on chasing
    public void UpdateChasingTarget(Vector3 seenPosition)
    {
        if (math.abs(seenPosition.y) > math.abs(transform.position.y) + 4 || math.abs(seenPosition.y) < math.abs(transform.position.y) - 4)  // If cat is too far above or too far below
        {
            seenPosition.y = transform.position.y;
        }
        currentState = NPCStates.Chasing;
        isAlertable = false;
        chaseTarget = seenPosition;
        chaseLastUpdated = Time.time;
    }

    private void GoTowardsChaseTarget()
    {
        // Walk towards the target
        // If close enough to the cat, record consecutive amount of time within capturing range
        // If not close enough to the cat but reached the destination, record we have reached dest and after we've been at destination for a time, end chase
        if (chaseTarget == null)
        {
            Debug.Log("Nothing to chase");
            currentState = NPCStates.Patrolling;
        }

        agent.SetDestination(chaseTarget);

        // Only focus on chasing, Capturing detect handled in NPC Detection
        // If we are at destination and no update in a certain amount of time, End Chase.
        float dist = GetDistance(chaseTarget, transform.position);
        if (dist <= destinationLeniency && Time.time > chaseLastUpdated + aggroDuration)
        {
            EndChase();
        }
    }

    // Ends chase, enables alerts and then proceeds to either Patrol or Alert
    private void EndChase()
    {
        isAlertable = true;
        currentState = NPCStates.Surveying;
        enemyDet.EndChase();
    }

    [PunRPC]
    public void CaptureCat(int caughtID)
    {
        Debug.Log("Capture!");
        GameObject player = PhotonView.Find(caughtID).gameObject;
        
        SetCarriedCat(player); 
        
        // Teleport cat to the carry point
        carriedCat.transform.SetLocalPositionAndRotation(catCarryPoint.transform.position, catCarryPoint.transform.rotation);
        carriedCat.transform.SetParent(gameObject.transform);

        // Make Cat not visible
        ToggleCatVisibility(false);
        
        // Enter Carry mode
        EnterCarryMode();
    }

    private void SetCarriedCat(GameObject player)
    {
        // Disable cat controls and physics
        if (player.TryGetComponent<PlayerController>(out PlayerController pc))
        {   // Get Photon View of the player
            carriedCat = pc;
            // Make cat not visible to detection?
            if (pc.photonView.IsMine)
            {   // Only disable stuff if it is our player, everybody else's is already disabled
                ToggleOwnDisabled(true);
            }
        }
        else
        {
            Debug.Log("No PhotonView found!");
        }   
    }

    private void ToggleOwnDisabled(bool s)
    {
        if (carriedCat.TryGetComponent<PlayerMovement>(out PlayerMovement pm))
        {   // Disable player movement
            pm.LockMove(s);
        }
        else
        {
            Debug.Log("No Player Movement attached!");
        }

        if (carriedCat.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {   // Disable rigidbody
            rb.isKinematic = s;
        }
        else
        {
            Debug.Log("No Rigidbody!");
        }
    }

    private void ToggleCatVisibility(bool vis)
    {
        carriedCat.isVisible = vis;
    }

    private void EnterCarryMode()
    {
        // Enter Carry mode
        currentState = NPCStates.Carrying;
        EndChase();
        isAlertable = false;
        enemyDet.SetCarrying(true);
        RouteToNearestCage();
    }

    private void RouteToNearestCage()
    {
        float shortestDist = -1;
        for (int i = 0; i < cages.Length; i++)
        {
            float dist = GetDistance(transform.position, cages[i].transform.position);
            if (dist < shortestDist || shortestDist < 0)
            {   // If distance is shorter than the shortest recorded or if it is not yet initialised
                nearestCageIDX = i;
                shortestDist = dist;
            }
        }
        Debug.Log("Nearest cage at idx " + nearestCageIDX);
    }

    private void CarryCat()
    {
        // Make sure cat is synced as well
        // Otherwise should be able to simply walk towards specific cage
        Vector3 goal = cages[nearestCageIDX].transform.position;
        agent.destination = goal;
        // Check if arrived at cage, if yes: put cat in cage and administer any punishment
        float dist = GetDistance(goal, carriedCat.transform.position);
        if (dist <= destinationLeniency)
        {
            Debug.Log("Putting cat in cage");
            photonView.RPC(nameof(PutCatInCage), RpcTarget.All);
            // Return to patrolling
            currentState = NPCStates.Surveying;
        }
    }

    [PunRPC]
    private void PutCatInCage()
    {
        // Place cat on the CagePoint
        carriedCat.transform.SetParent(null);
        carriedCat.transform.SetLocalPositionAndRotation(cages[nearestCageIDX].transform.position, cages[nearestCageIDX].transform.rotation);

        cages[nearestCageIDX].LockCat(carriedCat);

        ReenableMovement();
        PostCageCleanup();
    }

    private void ReenableMovement()
    {
        // if cat .IsMine, reenable rb and movement
        if (carriedCat.photonView.IsMine)
        {
            ToggleOwnDisabled(false);
        }
    }

    private void PostCageCleanup()
    {
        nearestCageIDX = -1;
        isAlertable = true;
        enemyDet.SetCarrying(false);
    }

    // Potentially a DropCat if we want to stun the NPC and save a cat being carried
}
