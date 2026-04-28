using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SoundManager : MonoBehaviourPun
{
    public static SoundManager instance;

    [SerializeField] private AudioClip[] meowSounds;
    [SerializeField] private AudioClip[] objectBrokenSounds;
    [SerializeField] private AudioClip[] paperRipSounds;
    [SerializeField] [Range(0f, 1f)] private float meowVolume;
    [SerializeField] [Range(0f, 1f)] private float objectBrokenVolume;
    [SerializeField] [Range(0f, 1f)] private float paperRipVolume;

    private readonly HashSet<PlayerController> subscribedPlayers = new();
    private enum SoundType
    {
        Meow,
        Object,
        Paper
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

    public void SubscribeToObjects()
    {
        InteractableManager.instance.OnBreakEvent += OnObjectBroken;
    }

    public void UnSubscribeToObjects()
    {
        InteractableManager.instance.OnBreakEvent -= OnObjectBroken;
    }

    private void OnPlayerMeow(Vector3 location)
    {
        int index = Random.Range(0, meowSounds.Length);

        photonView.RPC(nameof(PlaySound), RpcTarget.All, SoundType.Meow, index, location);
    }

    private void OnObjectBroken(int actorNumber, ObjectType objectType, Vector3 location, string tag)
    {
        switch (tag)
        {
            case "Painting":
            case "Document":
                int paperidx = Random.Range(0, paperRipSounds.Length);
                photonView.RPC(nameof(PlaySound), RpcTarget.All, SoundType.Paper, paperidx, location);
                break;
            default:
                int index = Random.Range(0, objectBrokenSounds.Length);
                photonView.RPC(nameof(PlaySound), RpcTarget.All, SoundType.Object, index, location);
                break;
        }
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
            case SoundType.Paper:
                library = paperRipSounds;
                volume = paperRipVolume;
                break;
            default:
                break;
        }

        if (library == null || library.Length == 0) return;
        if (index < 0 || index >= library.Length) return;

        AudioClip soundClip = library[index];
        if (soundClip == null) return;

        AudioSource.PlayClipAtPoint(soundClip, location, volume);
    }
}
