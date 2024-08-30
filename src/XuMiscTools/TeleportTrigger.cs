using System;
using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace XuMiscTools;

[RequireComponent(typeof(AudioSource))]
public class TeleportTrigger : NetworkBehaviour
{
    #region Enums

    public enum ForceDirection
    {
        Forward,
        Backward,
        Up,
        Down,
        Left,
        Right,
        Center,
    }

    #endregion

    #region Fields
    [Header("Teleport Settings")]
    [Tooltip("Teleport Triggers to teleport to.")]
    public List<TeleportTrigger> otherTriggers = new List<TeleportTrigger>();
    [Tooltip("OPTIONAL position to teleport to (uses the otherTrigger's forward vector if not set).")]
    public GameObject? positionForTeleport = null;
    [Space(2)]

    [Header("Force Settings")]
    [Tooltip("The force direction of the damage.")]
    public ForceDirection forceDirection = ForceDirection.Forward;
    [Tooltip("Strength of the directional force.")]
    public float forceMagnitude = 1f;
    [Tooltip("The force magnitude on THE PLAYER after the teleport.")]
    public float forceMagnitudeAfterTeleport = 0f;
    [Space(2)]

    [Header("Trigger Settings")]
    [Tooltip("Whether to trigger for enemies.")]
    public bool triggerForEnemies = false;
    [Tooltip("Whether to trigger for players.")]
    public bool triggerForPlayers = false;
    [Space(2)]

    [Header("Audio Settings")]
    [Tooltip("This object's own audiosource")]
    public AudioSource audioSource = null!;
    [Tooltip("Whether to play sound at teleport position that enemies can hear.")]
    public bool soundAttractsDogs = false;
    [Tooltip("Damage clip to play when damage is dealt to player/enemy.")]
    public List<AudioClip> teleportClips = new List<AudioClip>();


    private List<AudioSource> audioSources = new List<AudioSource>();
    private System.Random random = new System.Random();
    private InteractTrigger? interactTrigger = null;
    // No audiosource cuz expects you to put an audiosource on this gameobject

    #endregion
    private void OnEnable()
    {
        foreach (TeleportTrigger otherTrigger in otherTriggers) {
            audioSources.Add(otherTrigger.audioSource);
        }
        random = new System.Random(StartOfRound.Instance.randomMapSeed + 85);
        interactTrigger?.onInteract.AddListener(OnInteract);
    }

    public void OnInteract(PlayerControllerB player) {
        if (GameNetworkManager.Instance.localPlayerController != player) return;
        TeleportPlayerServerRpc(Array.IndexOf(StartOfRound.Instance.allPlayerScripts, player));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerForPlayers && other.CompareTag("Player") && GameNetworkManager.Instance.localPlayerController == other.TryGetComponent<PlayerControllerB>(out PlayerControllerB player))
        {
            TeleportPlayer(player);
        }
        else if (triggerForEnemies)
        {
            Transform? parent = TryFindRoot(other.transform);
            if (parent != null && parent.TryGetComponent<EnemyAI>(out EnemyAI enemy) && !enemy.isEnemyDead)
            {
                Vector3 position;
                int index = random.Next(0, teleportClips.Count);
                Vector3 forceDirectionVector = CalculateForceDirection(forceMagnitude, index);
                if (positionForTeleport != null)
                {
                    position = positionForTeleport.transform.position + forceDirectionVector;
                }
                else
                {
                    position = otherTriggers[index].transform.position + forceDirectionVector;
                }

                if (soundAttractsDogs) {
                    RoundManager.Instance.PlayAudibleNoise(audioSources[index].transform.position, audioSources[index].maxDistance, audioSources[index].volume, 0, false, 0);
                }
                RoundManager.PlayRandomClip(audioSources[index], teleportClips.ToArray(), true, audioSources[index].volume, 0, teleportClips.Count);
                enemy.agent.Warp(RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, 2.5f));
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TeleportPlayerServerRpc(int playerIndex) {
        TeleportPlayerClientRpc(playerIndex);
    }

    [ClientRpc]
    public void TeleportPlayerClientRpc(int playerIndex) {
        TeleportPlayer(StartOfRound.Instance.allPlayerScripts[playerIndex]);
    }

    public void TeleportPlayer(PlayerControllerB player)
    {
        int index = random.Next(0, otherTriggers.Count);
        Vector3 forceDirectionVector = CalculateForceDirection(forceMagnitude, index);
        if (positionForTeleport != null) {
            player.transform.position = positionForTeleport.transform.position + forceDirectionVector;
        } else {
            player.transform.position = forceDirectionVector + otherTriggers[index].transform.position;
        }

        if (soundAttractsDogs) {
            RoundManager.Instance.PlayAudibleNoise(audioSources[index].transform.position, audioSources[index].maxDistance, audioSources[index].volume, 0, false, 0);
        }
        audioSources[index].PlayOneShot(teleportClips[index], audioSources[index].volume);

        Vector3 forceAfterTeleport = CalculateForceDirection(forceMagnitudeAfterTeleport, index);
        player.externalForces += forceAfterTeleport;
    }

    private Vector3 CalculateForceDirection(float baseForce, int index)
    {
        TeleportTrigger otherTrigger = otherTriggers[index];
        Vector3 forceDirectionVector = Vector3.zero;
        if (positionForTeleport != null)
        {
            return Vector3.zero;
        }
        switch (forceDirection)
        {
            case ForceDirection.Forward:
                forceDirectionVector = otherTrigger.transform.forward;
                break;
            case ForceDirection.Backward:
                forceDirectionVector = -otherTrigger.transform.forward;
                break;
            case ForceDirection.Up:
                forceDirectionVector = otherTrigger.transform.up;
                break;
            case ForceDirection.Down:
                forceDirectionVector = -otherTrigger.transform.up;
                break;
            case ForceDirection.Left:
                forceDirectionVector = -otherTrigger.transform.right;
                break;
            case ForceDirection.Right:
                forceDirectionVector = otherTrigger.transform.right;
                break;
            case ForceDirection.Center:
                forceDirectionVector = Vector3.zero;
                break;
        }

        return forceDirectionVector.normalized * baseForce;
    }

    public static Transform? TryFindRoot(Transform child)
    {
        Transform current = child;
        while (current != null)
        {
            if (current.GetComponent<NetworkObject>() != null)
            {
                return current;
            }
            current = current.transform.parent;
        }
        return null;
    }
}