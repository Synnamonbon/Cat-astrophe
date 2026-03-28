using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCoolDown = 0.5f; 
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackRotationDuration = 10f;
    [SerializeField] private float pushForce = 20f;
    private bool canAttack = true;
    private BoxCollider boxCollider;
    private Rigidbody playerRB;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        playerRB = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        InputManager.instance.OnAttack += Attack;
        boxCollider.enabled = false;
    }

    private void Attack(object sender, EventArgs e)
    {
        if (!canAttack) return;
        if (!playerMovement.isGrounded) return;
        Debug.Log("Attack initiated");
        StartCoroutine(RotateToCamera());
    }

    private IEnumerator EnableBoxCollider()
    {
        yield return new WaitForSeconds(0.3f);
        boxCollider.enabled = true;

        //Debug.Log("Player is attacking");
        yield return new WaitForSeconds(attackDuration);
        boxCollider.enabled = false;
        playerMovement.LockMove(false);

        yield return new WaitForSeconds(attackCoolDown);
        canAttack = true;
    }

    private IEnumerator RotateToCamera()
    {
        canAttack = false;
        playerMovement.LockMove(true);

        Quaternion startDirection = transform.rotation;
        Quaternion targetDirection = playerMovement.GetCameraForward();
        
        float elapsed = 0f;

        while (elapsed < attackRotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / attackRotationDuration;

            transform.rotation = Quaternion.Slerp(startDirection, targetDirection, t);
            yield return null;
        }

        transform.rotation = targetDirection;
        StartCoroutine(EnableBoxCollider());
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ObjectPush>(out ObjectPush obj))
        {
            TransferOwnership(other.gameObject);
            Vector3 currentPos = playerRB.transform.position;
            obj.ObjectGotHit(pushForce, currentPos);
        }
    }

    private void TransferOwnership(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<PhotonView>(out PhotonView pv))
            {
                if (!pv.IsMine && pv != null)
                {
                    pv.TransferOwnership(PhotonNetwork.LocalPlayer);
                    Debug.Log("Transferred ownership");
                }
            }
    }
}
