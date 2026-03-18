using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyNPCNavigation : MonoBehaviourPunCallbacks
{
    [NonSerialized] public NavMeshAgent agent;
    [NonSerialized] private List<Vector3> patrolRoute = new List<Vector3>();             // Patrol route
    public GameObject routeHolder;                                                          // Holder for patrol route points
    private int currentWaypoint = 0;

     [Header("Patrol Balancing")]
    [SerializeField] private float destinationLeniency = 1.0f;
    [SerializeField] private float surveyDuration = 2f;
    [SerializeField] private float patrolSpeed = 4f;
    [SerializeField] private float alertSpeed = 6.5f;

    private bool isLooking = false;
    private NPCStates currentState;
    private List<Vector3> alertSources = new List<Vector3>();
    private bool isAlertable = true;              // To be used to determine if NPC can be distracted right now. Turn off when chasing/carrying or if they are idling/lazing around?

    private void OnEnable()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = destinationLeniency/2;

        GameObject[] routes = GameObject.FindGameObjectsWithTag("Route");
        routeHolder = routes[0];

        foreach(Transform child in routeHolder.GetComponentsInChildren<Transform>())
        {
            patrolRoute.Add(child.position);
        }
        patrolRoute.Remove(routeHolder.GetComponent<Transform>().position);

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
                    if (alertSources.Count == 0)
                    {
                        currentState = NPCStates.Patrolling;
                    }
                    else
                    {
                        currentState = NPCStates.Alerted;
                    }
                }
                break;
            case NPCStates.Chasing:
                GoTowardsChaseTarget();
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
            Debug.Log("Where's my patrol path :<");
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
    // 
    public void StartChasingTarget()
    {
        currentState = NPCStates.Chasing;
        isAlertable = false;
    }

    private void GoTowardsChaseTarget()
    {

    }
}
