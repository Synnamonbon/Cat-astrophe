using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCoolDown = 0f; // will implement later
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float pushForce = 20f;
    private bool canAttack = true;
    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
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
        boxCollider.enabled = true;
        Debug.Log("Player is attacking");
        yield return new WaitForSeconds(attackDuration);
        boxCollider.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BreakableObject>(out BreakableObject b))
        {
            b.GetPushed(gameObject.transform.position, pushForce);
            //Debug.Log("Push");
        }
    }
}
