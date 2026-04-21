using System;
using System.Collections;
using UnityEngine;

public class PlayerMeow : MonoBehaviour
{
    [SerializeField] private float meowCooldown = 0.5f;
    private bool canMeow;

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
            StartCoroutine(MeowCoroutine()); 
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
