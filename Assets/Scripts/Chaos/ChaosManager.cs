using UnityEngine;

public class ChaosManager : MonoBehaviour
{
    public static ChaosManager instance;
    // Tracks numeric value of Chaos Meter, will need to connect to UI eventually.
    // On scene start we must set the target value for chaos meter according to the number of players we have
    // Then we have to add an increase chaos meter function
    private int currentChaos;
    private int targetChaos;
    // Task abstract class with event "Condition", int counter and int targetValue

    // Subscribe to Object Manager's "SomethingBroke" Action with argument in playerID and EnumObjectType objectType

    private void Awake()
    {
        SingletonPattern();
        SubscribeToChaosEvents();
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

    private void SubscribeToChaosEvents()
    {
        ObjectManager.instance.OnBreakEvent += BreakPoints;
    }

    private void BreakPoints(int playerID, ObjectType objectType)
    {
        // Update points according to object type
        // Check for tasks requiring BreakEvent
    }

    private void UpdateCurrentPoints(int pts)
    {
        currentChaos += pts;
    }
}
