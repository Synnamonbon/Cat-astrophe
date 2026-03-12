using UnityEngine;

public class ObjectPush : MonoBehaviour
{
    private Rigidbody objectRB;

    private void Awake()
    {
        objectRB = GetComponent<Rigidbody>();
    }

    public void ObjectGotHit(float force, Vector3 forceLocation)
    {
        objectRB.constraints = RigidbodyConstraints.None;
        ApplyForce(force, forceLocation);
    }

    private void ApplyForce(float forceSize, Vector3 forceLocation)
    {
        Vector3 direction = (objectRB.position - forceLocation).normalized;
        direction.y += 0.3f;
        direction.Normalize();

        objectRB.AddForce(direction * forceSize, ForceMode.Impulse);
        //Debug.Log(force);
        //Debug.Log(forceLocation);
    }
}
