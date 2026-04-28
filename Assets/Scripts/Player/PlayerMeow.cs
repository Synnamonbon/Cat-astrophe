using System;
using System.Collections;
using UnityEngine;

public class PlayerMeow : MonoBehaviour
{
    [SerializeField] private float meowCooldown = 0.5f;
    [SerializeField] private float meowAlert = 10f;
    private float closenessDistance = 4f;
    private bool canMeow;

    public event Action<string> OnMeowAndDistance;     // layer of entity
    public event Action<Transform, float> MeowAlert;

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        InputManager.instance.OnMeow += Meow;
        canMeow = true;
    }

    private void OnDestroy()
    {
        InputManager.instance.OnMeow -= Meow;
    }

    private void Meow(object sender, EventArgs e)
    {
        if (playerController == null) return;
        if (canMeow)
        {
            MeowAlert?.Invoke(transform, meowAlert);
            StartCoroutine(MeowCoroutine()); 
            CheckNearbyPlayers();
            CheckHeight();
        }
    }

    private void CheckNearbyPlayers()
    {
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (Vector3.Distance(gameObject.transform.position, player.transform.position) <= closenessDistance)
            {
                OnMeowAndDistance?.Invoke("Player");
            }
        }
    }

    private void CheckHeight()
    {
        GameObject ground = GameObject.FindGameObjectsWithTag("Ground")[0];
        if (gameObject.transform.position.y >= ground.transform.position.y + closenessDistance/2)
        {
            OnMeowAndDistance?.Invoke("Ground");
        }
    }

    private IEnumerator MeowCoroutine()
    {
        canMeow = false;
        playerController.MakePlayerMeow();
        yield return new WaitForSeconds(meowCooldown);
        canMeow = true;
    }
}
