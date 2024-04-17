using InnerNet;
using PowerTools;
using UnityEngine;
using UnityEngine.Serialization;

public partial class ShipStatus : InnerNetObject
{
	public Color CameraColor = Color.black;
	public float MaxLightRadius = 5f;
	public float MinLightRadius = 1f;
	public float MapScale = 4.4f;
	public MapBehaviour MapPrefab;
	public ExileController ExileCutscenePrefab;
	// public OverlayKillAnimation EmergencyOverlay; // set in code
	// public OverlayKillAnimation ReportOverlay; // set in code
	public Vector2 InitialSpawnCenter;
	public Vector2 MeetingSpawnCenter;
	public Vector2 MeetingSpawnCenter2;
	public float SpawnRadius = 1.55f;
	public NormalPlayerTask[] CommonTasks;
	public NormalPlayerTask[] LongTasks;
	[FormerlySerializedAs("NormalTasks")] public NormalPlayerTask[] ShortTasks;
	public PlayerTask[] SpecialTasks;
	public Transform[] DummyLocations;
	public SurvCamera[] AllCameras;
	public PlainDoor[] AllDoors;
	public Console[] AllConsoles;
	public StringNames[] SystemNames;
	public AudioClip SabotageSound;
	public AnimationClip[] WeaponFires;
	public SpriteAnim WeaponsImage;
	public AudioClip[] VentMoveSounds;
	public AudioClip VentEnterSound;
	public AnimationClip HatchActive;
	public SpriteAnim Hatch;
	public ParticleSystem HatchParticles;
	public AnimationClip ShieldsActive;
	public SpriteAnim[] ShieldsImages;
	public SpriteRenderer ShieldBorder;
	public Sprite ShieldBorderOn;
	public MedScannerBehaviour MedScanner;
	public float Timer;
	public float EmergencyCooldown;
	public MapType Type;
}
