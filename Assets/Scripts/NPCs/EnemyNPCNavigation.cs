using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNPCNavigation : MonoBehaviour
{
    public NavMeshAgent agent;
    public List<Transform> patrolRoute;             // Patrol route
    public GameObject routeHolder;                  // Holder for patrol route points
    public int currentWaypoint = 0;
    public float distBeforeNext = 1.0f;
    private bool isLooking = false;
    private NPCStates currentState;
    private List<Transform> alertSources;
    private bool alertable = true;              // To be used to determine if NPC can be distracted right now. 

    void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = distBeforeNext/2;
        foreach(Transform child in routeHolder.GetComponentsInChildren<Transform>())
        {
            patrolRoute.Add(child);
        }
        patrolRoute.Remove(routeHolder.GetComponent<Transform>());
        currentState = NPCStates.Patrolling;
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case NPCStates.Patrolling:
                Patrol();
                break;
            case NPCStates.Alerted:
                // Naively Assume we have an alert source for now
                GoToSoundSource();
                break;
            case NPCStates.Surveying:
                // we want to start looking around only once. When that is done we want to return to the previous state, Alerted or Patrolling.
                if (!isLooking)
                {
                    if (alertSources.Count == 0)
                    {
                        // Increment Patrol Point here instead?
                        currentState = NPCStates.Patrolling;
                    }
                    else
                    {
                        currentState = NPCStates.Alerted;
                    }
                }
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
            return;
        }

        float dist = GetDistance(patrolRoute[currentWaypoint].position, transform.position);

        if (dist < distBeforeNext && !isLooking)
        {
            currentState = NPCStates.Surveying;
            StartCoroutine(LookAround());
        }
        
        agent.SetDestination(patrolRoute[currentWaypoint].position);
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
        float startRotation = transform.eulerAngles.y;
        float endRotation = startRotation + 360.0f;
        float t = 0.0f;
        while (t  < 2.0f)
        {
            t += Time.deltaTime;
            float yRotation = Mathf.Lerp(startRotation, endRotation, t / 2.0f) % 360.0f;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
            yield return null;
        }
        isLooking = false;
        agent.isStopped = false;
        IncrementPatrolPoint();
    }

    public void AlertToSound(Transform source)
    {
        // Behaviour to being alerted to a sound.
        // Currently just stop the current path and go to the source of the object.
        // If new Alert comes, empty list and set newest source at low chaos
        // At high chaos, add to end of list but keep pursuing first.
        if (alertSources.Count > 0){
            alertSources = new List<Transform>();
        }
        alertSources.Add(source);
        currentState = NPCStates.Alerted;
    }

    private void GoToSoundSource()
    {
        
    }

    private float GetDistance(Vector3 goal, Vector3 source)
    {
        return Vector3.Distance(goal, source);
    }
}
