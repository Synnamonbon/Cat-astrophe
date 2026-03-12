using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNPCNavigation : MonoBehaviour
{
    public NavMeshAgent agent;
    public List<Transform> waypoints;
    public GameObject wpHolder;
    public int currentWaypoint = 0;
    public float distBeforeNext = 1.0f;
    private bool isLooking = false;

    void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = distBeforeNext/2;
        foreach(Transform child in wpHolder.GetComponentsInChildren<Transform>())
        {
            waypoints.Add(child);
        }
        waypoints.Remove(wpHolder.GetComponent<Transform>());
    }

    // Update is called once per frame
    void Update()
    {
        Walk();
    }

    private void Walk()
    {
        if(waypoints.Count == 0)
        {
            return;
        }

        float dist = Vector3.Distance(waypoints[currentWaypoint].position, transform.position);

        if (dist < distBeforeNext && !isLooking)
        {
            StartCoroutine(UpdateWaypoint());
        }

        agent.SetDestination(waypoints[currentWaypoint].position);
    }

    private IEnumerator UpdateWaypoint()
    {
        isLooking = true;
        Debug.Log("Looking");
        yield return new WaitForSeconds(2.0f);
        currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
        isLooking = false;
    }
}
