using Photon.Pun;
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
        Debug.Log("Pushing object");
        objectRB.constraints = RigidbodyConstraints.None;
        Vector3 direction = (objectRB.position - forceLocation).normalized;
        direction.y += 0.3f;
        direction.Normalize();

        objectRB.AddForce(direction * force, ForceMode.Impulse);
    }
}
