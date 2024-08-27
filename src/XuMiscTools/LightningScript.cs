using System.Collections;
using System.Collections.Generic;
using DigitalRuby.ThunderAndLightning;
using UnityEngine;
using static UnityEngine.ParticleSystem;
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

    [Header("Pre-lightning strike settings")]
    [Tooltip("Whether to play the pre-lightning strike effect, this delays when lightning strikes.")]
    public bool PlayPreLightningEffects = true;
    [Space(2)]

    [Header("Explosion settings.")]
    [Tooltip("Range of the lethal killing explosion.")]
    public float KillRange = 1f;
    [Tooltip("Range of the non-lethal damage dealt to the player.")]
    public float DamageRange = 5f;
    [Tooltip("Damage dealt to the player when the lightning strikes.")]
    public int NonLethalDamage = 20;
    [Tooltip("Force from the explosion towards the player.")]
    public float PhysicsForce = 5f;
    [Tooltip("Whether to play the explosion effects.")]
    public bool PlayExplosionEffects = true;
    [Tooltip("Prefab to override the default explosion particle effect.")]
    public GameObject? OverridePrefab = null;
    [Tooltip("Whether lightning strikes through cars.")]
    public bool HitsThroughCar = false;

    private System.Random random = new System.Random();
    private StormyWeather stormy = null!;
    private LightningBoltPrefabScript localLightningBoltPrefabScript = null!;
    private AudioSource audioSource = null!;
    public void OnEnable() {
        random = new System.Random(StartOfRound.Instance.randomMapSeed + 85);
        stormy = UnityEngine.Object.FindObjectOfType<StormyWeather>(true);
        localLightningBoltPrefabScript = Object.Instantiate(stormy.targetedThunder);
        localLightningBoltPrefabScript.enabled = true;
        audioSource = Object.Instantiate(stormy.targetedStrikeAudio);
        
        if (stormy == null)
        {
            Plugin.Log.LogWarning("StormyWeather not found");
            return;
        }

        if (localLightningBoltPrefabScript == null)
        {
            Plugin.Log.LogWarning("localLightningBoltPrefabScript not found");
            return;
        }

        StartCoroutine(SpawnLightningBoltCoroutine());
    }

    private IEnumerator SpawnLightningBoltCoroutine() {
        while (true) {
            Vector3 positionToStrike = DetermineStrikePosition();
            StartCoroutine(SpawnLightningBolt(positionToStrike));
            yield return new WaitForSeconds(Mathf.Clamp(StrikeInterval, 0, 9999f));
        }
    }

    public IEnumerator SpawnLightningBolt(Vector3 strikePosition)
    {
        if (PlayPreLightningEffects)
        {
            audioSource.clip = stormy.staticElectricityAudio;
            ParticleSystem particleSystem = stormy.staticElectricityParticle;
            ShapeModule shape = particleSystem.shape;
            shape.meshRenderer = null;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
        }
        Vector3 offset = new Vector3((float)(random.NextDouble()*64-32), 0f, (float)(random.NextDouble()*64-32));
        Vector3 vector = strikePosition + Vector3.up * 160f + offset;

        Plugin.Log.LogInfo($"{vector} -> {strikePosition}");

        localLightningBoltPrefabScript.GlowWidthMultiplier = GlowWidthMultiplier;
        localLightningBoltPrefabScript.DurationRange = new RangeOfFloats { Minimum = DurationMinimum, Maximum = DurationMaximum };
        localLightningBoltPrefabScript.TrunkWidthRange = new RangeOfFloats { Minimum = TrunkMinimum, Maximum = TrunkMaximum };
        localLightningBoltPrefabScript.Camera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
        localLightningBoltPrefabScript.Source.transform.position = vector;
        localLightningBoltPrefabScript.Destination.transform.position = strikePosition;
        localLightningBoltPrefabScript.AutomaticModeSeconds = 0.2f;
        localLightningBoltPrefabScript.Generations = NumberOfBolts;
        localLightningBoltPrefabScript.CreateLightningBoltsNow();
        audioSource.transform.position = strikePosition + Vector3.up * 0.5f;
        audioSource.enabled = true;

        if (GameNetworkManager.Instance.localPlayerController.isInsideFactory) {
            audioSource.volume = 0f;
        } else {
            audioSource.volume = VolumeController;
        }
        if (LightningStrikeSound != null && LightningStrikeSound.Count > 0) audioSource.clip = LightningStrikeSound[Random.Range(0, LightningStrikeSound.Count)];
        stormy.PlayThunderEffects(strikePosition, audioSource);
        Landmine.SpawnExplosion(strikePosition, PlayExplosionEffects, KillRange, DamageRange, NonLethalDamage, PhysicsForce, OverridePrefab, HitsThroughCar);
    }

    public Vector3 DetermineStrikePosition() {
        if (UseGameObjectPosition) {
            return RoundManager.Instance.GetRandomPositionInRadius(this.transform.position, 0, BoltRangeFromGameObject, random);
        } else if (BoltPositions != null && BoltPositions.Count > 0) {
            return RoundManager.Instance.GetRandomPositionInRadius(BoltPositions[random.Next(BoltPositions.Count)].transform.position, 0, BoltRangeFromGameObject, random);
        }
        return this.transform.position;
    }

    public void OnDisable() {
        StopAllCoroutines();
    }
}