using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

public class AlertManager : MonoBehaviour
{
    public static AlertManager instance;
    private List<GameObject> enemies = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        SingletonPattern();
        DontDestroyOnLoad(gameObject);
    }

    private void SingletonPattern()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnEnable()
    {
        if (instance.enemies is null)
        {
            instance.enemies = GameObject.FindGameObjectsWithTag("EnemyNPC").ToList();
        }
    }

    public void AddEnemy(GameObject enemy)
    {
        Debug.Log("Adding " + enemy.name);
        instance.enemies.Add(enemy);
    }

    public void RemoveEnemy(GameObject enemy)
    {
        instance.enemies.Remove(enemy);
    }

    public void AlertNPCsInRange(Transform source, float range)
    {
        Debug.Log(instance.enemies.Count + " enemies");
        foreach(GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(source.position, enemy.transform.position);

            // Range check to see if enemy would be alerted
            if (dist <= range)                  
            {
                if (enemy.TryGetComponent<EnemyNPCNavigation>(out EnemyNPCNavigation enemyNav))
                {
                    // Make this an RPC call instead.
                    enemyNav.AlertToSound(source);
                }
                else
                {
                    Debug.Log("No enemy NPC Navigation script attached to enemy " + enemy.name);
                }
            }
        }
    }
}
