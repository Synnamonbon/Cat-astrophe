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
            StartCoroutine(LookAround());
        }

        agent.SetDestination(waypoints[currentWaypoint].position);
    }

    private void UpdateWaypoint()
    {
        currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
    }

    private IEnumerator LookAround()
    {
        Debug.Log("Looking");
        isLooking = true;
        agent.isStopped = true;
        float startRotation = transform.eulerAngles.y;
        float endRotation = startRotation + 360.0f;
        float t = 0.0f;
        while ( t  < 2.0f)
        {
            t += Time.deltaTime;
            float yRotation = Mathf.Lerp(startRotation, endRotation, t / 2.0f) % 360.0f;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
            yield return null;
        }
        isLooking = false;
        agent.isStopped = false;
        UpdateWaypoint();
    }
}
