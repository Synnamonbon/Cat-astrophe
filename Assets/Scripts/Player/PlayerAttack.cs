using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCoolDown = 0.5f; 
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float pushForce = 20f;
    private bool canAttack = true;
    private BoxCollider boxCollider;
    private Rigidbody playerRB;

    //

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        playerRB = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        InputManager.instance.OnAttack += Attack;
        boxCollider.enabled = false;
    }

    private void Attack(object sender, EventArgs e)
    {
        if (!canAttack)return;
        StartCoroutine(EnableBoxCollider());
    }

    private IEnumerator EnableBoxCollider()
    {
        canAttack = false;
        boxCollider.enabled = true;

        //Debug.Log("Player is attacking");
        yield return new WaitForSeconds(attackDuration);
        boxCollider.enabled = false;

        yield return new WaitForSeconds(attackCoolDown);
        canAttack = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ObjectPush>(out ObjectPush obj))
        {
            Vector3 currentPos = playerRB.transform.position;
            obj.ObjectGotHit(pushForce, currentPos);
        }
    }
}
