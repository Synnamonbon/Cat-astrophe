using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using Photon.Pun;

public class EnemyNPCDetection : MonoBehaviourPunCallbacks
{
    public GameObject visionOrigin;
    private List<GameObject> playersList;
    [Header("Detection tinkering:")]
    [Range(0f, 20f)]
    [SerializeField] private float visionDistance = 5f;
    [Range(5f, 80f)]
    [SerializeField] private float visionAngle = 50f;

    private void OnEnable()
    {
        playersList = GameObject.FindGameObjectsWithTag("Player").ToList();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DetectPlayers();
    }

    public void ResubscribeEnemy()
    {
        playersList = GameObject.FindGameObjectsWithTag("Player").ToList();
    }

    private GameObject[] DetectPlayers()
    {
        foreach (GameObject player in playersList)
        {
            Vector3 dir = player.transform.position - visionOrigin.transform.position;
            RaycastHit hit;
            if (Physics.Raycast(visionOrigin.transform.position, dir, out hit, visionDistance))         // Distance handling
            {
                GameObject hitObj = hit.transform.gameObject;
                // Angle checking
                float theta = Vector3.Angle(transform.forward, dir);
                // Debug.Log(theta + " Degrees for " + player.name);
                if (theta > visionAngle)
                {
                    //Debug.Log("Too far to side to see " + player.name);
                    continue;
                }
                // Tag checking
                if (!hitObj.CompareTag("Player"))
                {
                    //Debug.Log("Not hitting Player for " + player.name);
                    continue;
                }
                Debug.DrawRay(visionOrigin.transform.position, dir.normalized * hit.distance, Color.yellow);
            }
            else
            {
                //Debug.Log("No Hit for " + player.name);
            }
        }

        return null;
    }

    // Probably want a function "SeeCat" that forces the NPCNavigation into chase and sends the last known location of the cat.
    // If the cat is within grabbing distance, grab and force into "Carrying" mode.
}
