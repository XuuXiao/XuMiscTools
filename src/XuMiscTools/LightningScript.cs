using System.Collections;
using System.Collections.Generic;
using DigitalRuby.ThunderAndLightning;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

namespace XuMiscTools;
public class LightningScript : MonoBehaviour {

    [Header("Sound/Audio Settings.")]
    [Tooltip("The volume of the lightning bolts, recommend to keep at 0.2 or lower.")]
    public float VolumeController = 0.2f;
    [Tooltip("Audio clip played when lightning strikes, if null, defaults to lightning sounds.")]
    public List<AudioClip>? LightningStrikeSound = new List<AudioClip>();
    [Space(2)]

    [Header("Position and strike settings.")]
    [Tooltip("Whether you'd want the position of the bolts to be determined based on a range from the GameObject or a predefined list of positions.")]
    public bool UseGameObjectPosition = true;
    [Tooltip("Range of the lightning bolts being striken from GameObject position.")]
    public float BoltRangeFromGameObject = 10f;
    [Tooltip("If using predefined list of positions, this is for random-ness of the lightning bolt strike positions from said positions.")]
    public List<GameObject>? BoltPositions;
    [Tooltip("Range of lightning bolts being striken from predefined list of positions.")]
    public float BoltRangeFromList = 0f;
    [Tooltip("Interval cooldown between each lightning strike.")]
    public float StrikeInterval = 5f;
    [Space(2)]

    [Header("Miscellanous settings.")]
    [Tooltip("Multiplier for the size of the glow's width.")]
    public float GlowWidthMultiplier = 1.5f;
    [Tooltip("The number of lightning bolts generated onto a spot.")]
    public int NumberOfBolts = 1;

    [Header("Duration of the lightning strike, split into a min and max duration.")]
    public float DurationMinimum = 0.6f;
    public float DurationMaximum = 1.2f;
    
    [Header("Length of the trunk of the lightning strike, split into a min and max length.")]
    public float TrunkMinimum = 0.6f;
    public float TrunkMaximum = 1.2f;

    private System.Random random = new System.Random();
    public void OnEnable() {
        random = new System.Random(StartOfRound.Instance.randomMapSeed + 85);
        StartCoroutine(SpawnLightningBoltCoroutine());
    }

    private IEnumerator SpawnLightningBoltCoroutine() {
        while (true) {
            Vector3 positionToStrike = DetermineStrikePosition();
            SpawnLightningBolt(positionToStrike);
            yield return new WaitForSeconds(Mathf.Clamp((float)random.NextDouble() + StrikeInterval, 0, 9999f));
        }
    }

    public void SpawnLightningBolt(Vector3 strikePosition)
    {
        Vector3 offset = new Vector3((float)(random.NextDouble()*64-32), 0f, (float)(random.NextDouble()*64-32));
        Vector3 vector = strikePosition + Vector3.up * 160f + offset;

        StormyWeather stormy = UnityEngine.Object.FindObjectOfType<StormyWeather>(true);
        if (stormy == null)
        {
            Plugin.Log.LogWarning("StormyWeather not found");
            return;
        }

        // Plugin.ExtendedLogging($"{vector} -> {strikePosition}");

        LightningBoltPrefabScript localLightningBoltPrefabScript = Object.Instantiate(stormy.targetedThunder);
        localLightningBoltPrefabScript.enabled = true;

        if (localLightningBoltPrefabScript == null)
        {
            Plugin.Log.LogWarning("localLightningBoltPrefabScript not found");
            return;
        }

        localLightningBoltPrefabScript.GlowWidthMultiplier = GlowWidthMultiplier;
        localLightningBoltPrefabScript.DurationRange = new RangeOfFloats { Minimum = DurationMinimum, Maximum = DurationMaximum };
        localLightningBoltPrefabScript.TrunkWidthRange = new RangeOfFloats { Minimum = TrunkMinimum, Maximum = TrunkMaximum };
        localLightningBoltPrefabScript.Camera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
        localLightningBoltPrefabScript.Source.transform.position = vector;
        localLightningBoltPrefabScript.Destination.transform.position = strikePosition;
        localLightningBoltPrefabScript.AutomaticModeSeconds = 0.2f;
        localLightningBoltPrefabScript.Generations = NumberOfBolts;
        localLightningBoltPrefabScript.CreateLightningBoltsNow();
        AudioSource audioSource = Object.Instantiate(stormy.targetedStrikeAudio);;
        audioSource.transform.position = strikePosition + Vector3.up * 0.5f;
        audioSource.enabled = true;

        if (GameNetworkManager.Instance.localPlayerController.isInsideFactory) {
            audioSource.volume = 0f;
        } else {
            audioSource.volume = VolumeController;
        }
        if (LightningStrikeSound != null && LightningStrikeSound.Count > 0) audioSource.clip = LightningStrikeSound[Random.Range(0, LightningStrikeSound.Count)];
        stormy.PlayThunderEffects(strikePosition, audioSource);
    }

    public Vector3 DetermineStrikePosition() {
        if (UseGameObjectPosition) {
            return RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(this.transform.position, BoltRangeFromGameObject, default);
        } else if (BoltPositions != null && BoltPositions.Count > 0) {
            return RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(BoltPositions[random.Next(BoltPositions.Count)].transform.position, BoltRangeFromList, default);
        }
        return this.transform.position;
    }

    public void OnDisable() {
        StopAllCoroutines();
    }
}