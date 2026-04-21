using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SoundManager : MonoBehaviourPun
{
    public static SoundManager instance;

    [SerializeField] private AudioClip[] meowSounds;
    [SerializeField] [Range(0f, 1f)] private float meowVolume;
    [SerializeField] private AudioClip[] objectBrokenSounds;
    [SerializeField] [Range(0f, 1f)] private float objectBrokenVolume;

    private readonly HashSet<PlayerController> subscribedPlayers = new();
    private enum SoundType
    {
        Meow,
        Object
    }

    private void Awake()
    {
        SingletonPattern();
    }

    private void SingletonPattern()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SubscribeToPlayer(PlayerController playerController)
    {
        
        if (subscribedPlayers.Contains(playerController)) return;

        playerController.PlayerMeow += OnPlayerMeow;
        subscribedPlayers.Add(playerController);
    }

    public void UnsubscribeFromPlayer(PlayerController playerController)
    {
        if (playerController == null) return;
        if (!subscribedPlayers.Contains(playerController)) return;

        //playerController.PlayerMeow -= OnPlayerMeow;
        subscribedPlayers.Remove(playerController);
        Debug.Log("Unsubscribed player");
    }

    private void OnPlayerMeow(Vector3 location)
    {
        int index = Random.Range(0, meowSounds.Length);

        photonView.RPC(nameof(PlaySound), RpcTarget.All, SoundType.Meow, index, location);
    }

    [PunRPC]
    private void PlaySound(SoundType soundType, int index, Vector3 location)
    {
        AudioClip[] library = null;
        float volume = 0f;

        switch (soundType)
        {
            case SoundType.Meow:
                library = meowSounds;
                volume = meowVolume;
                break;
            case SoundType.Object:
                library = objectBrokenSounds;
                volume = objectBrokenVolume;
                break;
        }

        if (library == null || library.Length == 0) return;
        if (index < 0 || index >= library.Length) return;

        AudioClip soundClip = library[index];
        if (soundClip == null) return;

        AudioSource.PlayClipAtPoint(soundClip, location, volume);
    }
}
