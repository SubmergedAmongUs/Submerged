using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities.Attributes;
using Submerged.Elevators;
using Submerged.Enums;
using Submerged.ExileCutscene;
using Submerged.Extensions;
using Submerged.Floors;
using Submerged.Floors.Objects;
using Submerged.Minigames.CustomMinigames.FixWiring.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using Submerged.SpawnIn;
using Submerged.Systems.BoxCat;
using Submerged.Systems.Elevator;
using Submerged.Systems.Oxygen;
using Submerged.Systems.SecuritySabotage;
using Submerged.Vents;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Submerged.Map;

[RegisterInIl2Cpp]
public sealed class SubmarineStatus(nint intPtr) : MonoBehaviour(intPtr)
{
    public static SubmarineStatus instance;

    public ShipStatus normalShip;
    public List<SubmarineElevator> elevators = [];
    public bool shakeOverridden;
    public GameObject vitalsPanel;
    public Transform referenceHolder;
    public float lightTimer;
    public bool lightFlickerActive;
    public AudioSource powerDownSound;
    public AudioSource powerUpSound;
    public SwitchSystem switchSystem;

    public MinigameProperties minigameProperties;
    public Tilemap2 aprilFoolsShadowSpritesHolder;

    private float _ventTransitionTimer;

    private static float CrewLightMod => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.CrewLightMod);
    private static float ImpostorLightMod => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod);

    public void Awake()
    {
        if (instance != null)
        {
            Error("SubmarineStatus is a singleton, but multiple instances were spawned!");
            Destroy(this);

            return;
        }

        instance = this;
        normalShip = GetComponent<ShipStatus>();

        referenceHolder = new GameObject("Submerged Reference Holder").transform;
        referenceHolder.parent = transform;
        referenceHolder.gameObject.SetActive(false);

        minigameProperties = gameObject.AddComponent<MinigameProperties>();
        minigameProperties.Awake();
        DestroyImmediate(GetComponent<DivertPowerMetagame>());
        aprilFoolsShadowSpritesHolder = transform.Find("MinigameProperties/April Fools Shadow Sprites").GetComponent<Tilemap2>();

        ResolveTaskMinigames(normalShip.CommonTasks);
        ResolveTaskMinigames(normalShip.LongTasks);
        ResolveTaskMinigames(normalShip.ShortTasks);
        ResolveTaskMinigames(normalShip.SpecialTasks);
        ResolveSystemConsoleMinigames(normalShip.GetComponentsInChildren<SystemConsole>());
        ResolveDoorMinigames();
        ResolveExileController();
        ResolveCooldownConsoles(normalShip.AllConsoles);

        normalShip.CommonTasks[0].Arrow = normalShip.CommonTasks[0].gameObject.GetComponentInChildren<ArrowBehaviour>();

        normalShip.InitialSpawnCenter *= 0.8f / 0.85f;
        normalShip.MeetingSpawnCenter *= 0.8f / 0.85f;
        normalShip.MeetingSpawnCenter2 *= 0.8f / 0.85f;

        vitalsPanel = Instantiate(MapLoader.Airship.transform.Find("Medbay/panel_vitals").gameObject, transform);
        vitalsPanel.transform.position = new Vector3(4.9882f, 32.8877f, 0.0366f);
        vitalsPanel.SetActive(true);
        FixMinigameClosing(vitalsPanel.GetComponent<SystemConsole>().MinigamePrefab);

        normalShip.EmergencyButton = transform.Find("TopFloor/Adm-Obsv-Loun-MR/TaskConsoles/console-mr-callmeeting").GetComponent<SystemConsole>();

        normalShip.AllVents = transform.GetComponentsInChildren<Vent>().ToArray();
        for (int i = 0; i < normalShip.AllVents.Length; i++) normalShip.AllVents[i].Id = i;

        normalShip.CameraColor = Color.black;

        gameObject.AddComponent<KcegListener>();

        Collider2D centralVentCollider = transform.Find("BottomFloor/LowerCentral/MapBase/ShittyVentCollider").GetComponent<Collider2D>();
        Collider2D enginesVentCollider = transform.Find("BottomFloor/Engines-Security/MapBase/ShittyCornerCollider").GetComponent<Collider2D>();

        foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
        {
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
            Physics2D.IgnoreCollision(player.Collider, centralVentCollider);
            Physics2D.IgnoreCollision(player.Collider, enginesVentCollider);
        }

        // From Base Game \/

        ResolveBaseGameMinigame(TaskTypes.FixLights, MapLoader.Skeld);
        ResolveBaseGameMinigame(TaskTypes.SubmitScan, MapLoader.Skeld);
        ResolveBaseGameMinigame(TaskTypes.UploadData, MapLoader.Airship);

        ResolveHudMap();

        normalShip.EmergencyOverlay = MapLoader.Skeld.EmergencyOverlay;
        normalShip.ReportOverlay = MapLoader.Skeld.ReportOverlay;
        normalShip.VentEnterSound = MapLoader.Skeld.VentEnterSound;
        normalShip.VentMoveSounds = MapLoader.Skeld.VentMoveSounds;
        normalShip.SabotageSound = MapLoader.Skeld.SabotageSound;

        if (TutorialManager.InstanceExists)
        {
            SystemConsole taskPicker = Instantiate(MapLoader.Skeld.transform.GetComponentsInChildren<SystemConsole>().First(c => c.FreeplayOnly));
            taskPicker.transform.position = new Vector3(5.2393f, -27.8891f, -0.002f);
            taskPicker.usableDistance = 1;
            FixMinigameClosing(taskPicker.MinigamePrefab);
        }

        int num = 0;
        foreach (NormalPlayerTask t in normalShip.CommonTasks) t.Index = num++;
        foreach (NormalPlayerTask t in normalShip.LongTasks) t.Index = num++;
        foreach (NormalPlayerTask t in normalShip.ShortTasks) t.Index = num++;

        Instantiate(MapLoader.Fungle.specialSabotage.screenTint).gameObject.AddComponent<SubmarineOxygenTint>();
    }

    private void Start()
    {
        this.StartCoroutine(CoAddShadows());
    }

    private void Update()
    {
        // Prevents player from being unable to vent if their vent movement coroutine is interrupted by whatever means
        if (VentPatchData.InTransition)
        {
            _ventTransitionTimer += Time.deltaTime;
            if (_ventTransitionTimer > 0.5f) VentPatchData.InTransition = false;
        }
        else
        {
            _ventTransitionTimer = 0;
        }

        if (switchSystem.ActualSwitches != switchSystem.ExpectedSwitches)
        {
            lightTimer += Time.deltaTime;
        }

        if (shakeOverridden) return;
        float shakeAmount = 0f;
        float shakePeriod = 0f;

        foreach (SubmarineElevator elevator in elevators)
        {
            shakeAmount = elevator.FollowerCameraShakeAmount > shakeAmount ? elevator.FollowerCameraShakeAmount : shakeAmount;
            shakePeriod = elevator.FollowerCameraShakePeriod > shakePeriod ? elevator.FollowerCameraShakePeriod : shakePeriod;
        }

        HudManager.Instance.PlayerCam.shakeAmount = shakeAmount;
        HudManager.Instance.PlayerCam.shakePeriod = shakePeriod;
    }

    public void OnEnable()
    {
        Camera.main!.backgroundColor = normalShip.CameraColor;
        FollowerCamera followerCam = Camera.main!.GetComponent<FollowerCamera>();
        if (followerCam) followerCam.shakeAmount = followerCam.shakePeriod = 0;

        normalShip.Systems = new ICG.Dictionary<SystemTypes, ISystemType>(ShipStatus.SystemTypeComparer.Instance.Cast<ICG.IEqualityComparer<SystemTypes>>());

        normalShip.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType().Cast<ISystemType>());
        normalShip.Systems.Add(SystemTypes.Doors, new DoorsSystemType().Cast<ISystemType>());
        normalShip.Systems.Add(SystemTypes.Electrical, (switchSystem = new SwitchSystem()).Cast<ISystemType>());
        normalShip.Systems.Add(SystemTypes.MedBay, new MedScanSystem().Cast<ISystemType>());
        normalShip.Systems.Add(SystemTypes.Reactor, new ReactorSystemType(45f, SystemTypes.Reactor).Cast<ISystemType>());
        normalShip.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType().Cast<ISystemType>());
        normalShip.Systems.Add(SystemTypes.Ventilation, new VentilationSystem().Cast<ISystemType>());

        normalShip.Systems.Add(CustomSystemTypes.UpperCentral, new SubmarineOxygenSystem(30f).Cast<ISystemType>());

        normalShip.Systems.Add(CustomSystemTypes.ElevatorHallwayLeft,
                                     new SubmarineElevatorSystem(CustomSystemTypes.ElevatorHallwayLeft, false, CustomSystemTypes.ElevatorHallwayRight).Cast<ISystemType>());
        normalShip.Systems.Add(CustomSystemTypes.ElevatorHallwayRight,
                                     new SubmarineElevatorSystem(CustomSystemTypes.ElevatorHallwayRight, true, CustomSystemTypes.ElevatorHallwayLeft).Cast<ISystemType>());
        normalShip.Systems.Add(CustomSystemTypes.ElevatorLobbyLeft,
                                     new SubmarineElevatorSystem(CustomSystemTypes.ElevatorLobbyLeft, true, CustomSystemTypes.ElevatorLobbyRight).Cast<ISystemType>());
        normalShip.Systems.Add(CustomSystemTypes.ElevatorLobbyRight,
                                     new SubmarineElevatorSystem(CustomSystemTypes.ElevatorLobbyRight, false, CustomSystemTypes.ElevatorLobbyLeft).Cast<ISystemType>());
        normalShip.Systems.Add(CustomSystemTypes.ElevatorService,
                                     new SubmarineElevatorSystem(CustomSystemTypes.ElevatorService, false).Cast<ISystemType>());

        normalShip.Systems.Add(CustomSystemTypes.SubmarineFloor, new SubmarinePlayerFloorSystem().Cast<ISystemType>());
        normalShip.Systems.Add(CustomSystemTypes.SecuritySabotage, new SubmarineSecuritySabotageSystem().Cast<ISystemType>());
        normalShip.Systems.Add(CustomSystemTypes.SpawnIn, new SubmarineSpawnInSystem().Cast<ISystemType>());
        normalShip.Systems.Add(CustomSystemTypes.BoxCat, new SubmarineBoxCatSystem().Cast<ISystemType>());

        normalShip.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(GetActivatableSystems(normalShip.Systems)).Cast<ISystemType>());

        normalShip.SystemNames = new[]
        {
            StringNames.FixComms,
            StringNames.FixLights,
            StringNames.AdminMapSystem,
            StringNames.SecurityCamsSystem,
            CustomStringNames.RetrieveOxygenMask,
            CustomStringNames.StabilizeWaterLevels
        };
    }

    private static Il2CppReferenceArray<IActivatable> GetActivatableSystems(ICG.Dictionary<SystemTypes, ISystemType> systems)
    {
        IEnumerable<IActivatable> enumerator()
        {
            foreach (ISystemType system in systems._values ??= new ICG.Dictionary<SystemTypes, ISystemType>.ValueCollection(systems))
            {
                if (system is Il2CppObjectBase iObj && iObj.TryCast<IActivatable>() is { } activatable)
                {
                    yield return activatable;
                }
            }
        }

        return enumerator().ToArray();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    [HideFromIl2Cpp]
    private static IEnumerator CoAddShadows()
    {
        while (!PlayerControl.LocalPlayer) yield return null;

        foreach (PlayerControl player in FindObjectsOfType<PlayerControl>())
        {
            player.transform.Find("BodyForms").gameObject.EnsureComponent<GenericShadowBehaviour>();
        }
    }

    [HideFromIl2Cpp]
    public float CalculateLightRadius(NetworkedPlayerInfo player, bool neutral = false, bool neutralImpostor = false)
    {
        if (switchSystem.ActualSwitches != switchSystem.ExpectedSwitches)
        {
            float amount = GetSabotagedLightRadiusAndPlaySounds(neutralImpostor || (!neutral && player != null && player.Role.IsImpostor));

            if (!neutral && (player == null || player.IsDead)) return normalShip.MaxLightRadius;

            // if (!neutral && player.Role.IsImpostor) return NormalShipStatus.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;
            return amount;
        }

        lightFlickerActive = false;

        if (!neutral && (player == null || player.IsDead)) return normalShip.MaxLightRadius;

        float num = switchSystem.Value / 255f;

        if (num >= 1f)
        {
            SoundManager.Instance.StopSound(minigameProperties.audioClips[1]);
            SoundManager.Instance.StopSound(minigameProperties.audioClips[2]);
            lightTimer = 0f;
        }

        if (neutralImpostor || (!neutral && player.Role.IsImpostor)) return normalShip.MaxLightRadius * ImpostorLightMod;

        return Mathf.Lerp(normalShip.MinLightRadius, normalShip.MaxLightRadius, num) * CrewLightMod;
    }

    [HideFromIl2Cpp]
    public float GetSabotagedLightRadiusAndPlaySounds(bool isImpostor)
    {
        lightFlickerActive = true;
        float[] segments =
        [
            0.2f, // Off
            0.3f, 0.15f, // Off
            0.35f, 0.2f, // Off
            0.8f, 0.1f, // Off,
            0.2f, 0.1f, 0.1f
        ];

        float segmentSum = segments.Sum();

        if (lightTimer > segmentSum - 0.4f)
        {
            if (!powerDownSound) powerDownSound = SoundManager.Instance.PlaySound(minigameProperties.audioClips[1], false);
        }

        if (lightTimer < segmentSum)
        {
            bool lightsOn = true;
            float currentSum = 0f;

            foreach (float time in segments)
            {
                if (lightTimer > currentSum)
                {
                    lightsOn = !lightsOn;
                    currentSum += time;
                }
                else
                {
                    if (!isImpostor)
                    {
                        if (lightsOn) return normalShip.MaxLightRadius * CrewLightMod;

                        return 0;
                    }

                    if (lightsOn) return normalShip.MaxLightRadius * ImpostorLightMod;

                    return normalShip.MaxLightRadius * ImpostorLightMod * 0.75f;
                }
            }
        }

        if (lightTimer < segmentSum + 2.50f)
        {
            if (isImpostor) return normalShip.MaxLightRadius * ImpostorLightMod * 0.75f;

            return 0;
        }

        if (!powerUpSound) powerUpSound = SoundManager.Instance.PlaySound(minigameProperties.audioClips[2], false);

        if (lightTimer < segmentSum + 3.75f)
        {
            if (isImpostor) return normalShip.MaxLightRadius * ImpostorLightMod * 0.75f;

            return 0;
        }

        float adjustedamount = lightTimer - segmentSum - 3.75f;

        if (adjustedamount >= 1)
        {
            lightFlickerActive = false;
        }

        if (isImpostor)
        {
            return normalShip.MaxLightRadius * ImpostorLightMod * Mathf.Lerp(0.75f, 1, adjustedamount);
        }

        return Mathf.Lerp(0, normalShip.MinLightRadius, Mathf.Clamp01(adjustedamount)) * CrewLightMod;
    }

    [HideFromIl2Cpp]
    public Sprite[] GetReplacementShadowSprites(string objectName)
    {
        return objectName switch
        {
            "Horse" or "LongBoiBody" or "LongSeekerBody" => aprilFoolsShadowSpritesHolder.sprites,
            _ => minigameProperties.sprites
        };
    }

    #region Resolve Stuff

    private void ResolveBaseGameMinigame(TaskTypes taskType, ShipStatus from)
    {
        IEnumerable<PlayerTask> ourTasks = GetAllTasks(normalShip);
        PlayerTask targetTask = GetAllTasks(from).First(t => t.TaskType == taskType);

        foreach (PlayerTask task in ourTasks.Where(t => t.TaskType == taskType))
        {
            task.MinigamePrefab = targetTask.MinigamePrefab;
            FixMinigameClosing(task.MinigamePrefab); // It's probably fine if we modify the prefab directly cuz this shouldn't interfere with base game
        }
    }

    private void FixMinigameClosing(Minigame minigame)
    {
        foreach (PassiveButton passiveButton in minigame.GetComponentsInChildren<PassiveButton>(true))
        {
            foreach (PersistentCall call in passiveButton.OnClick.m_PersistentCalls.m_Calls)
            {
                if (call.methodName == nameof(Minigame.Close) && call.mode == PersistentListenerMode.Bool && call.target.TryCast<Minigame>())
                {
                    call.mode = PersistentListenerMode.Void;
                }
            }
        }
    }

    private void ResolveHudMap()
    {
        // Clone hud map (not sure if this is still needed, but it was needed in the past)
        normalShip.MapPrefab.gameObject.SetActive(false);
        normalShip.MapPrefab = Instantiate(normalShip.MapPrefab);

        // Update font materials
        TextMeshPro basegameText = MapLoader.Skeld.MapPrefab.GetComponentInChildren<TextMeshPro>();
        TMP_FontAsset mapFont = basegameText.font;
        Material mapFontMaterial = basegameText.fontMaterial;
        foreach (TextMeshPro text in normalShip.MapPrefab.GetComponentsInChildren<TextMeshPro>())
        {
            text.font = mapFont;
            text.fontMaterial = mapFontMaterial;
        }
    }

    [HideFromIl2Cpp]
    private void ResolveTaskMinigames<T>(Il2CppReferenceArray<T> tasks) where T : PlayerTask
    {
        for (int i = 0; i < tasks.Length; i++)
        {
            T currentTask = tasks[i];

            if (currentTask == null) continue;
            if (currentTask.MinigamePrefab == null) continue;

            tasks[i] = Instantiate(tasks[i], referenceHolder);
            tasks[i].MinigamePrefab = Instantiate(tasks[i].MinigamePrefab, referenceHolder);

            PlayerTask task = tasks[i];

            Minigame minigame = task.MinigamePrefab;
            if (!minigame) continue;

            StowArms propertiesObj = minigame.GetComponentInChildren<StowArms>(true);
            if (!propertiesObj || minigame.GetComponentInChildren<MinigameProperties>()) continue;

            (string playerTaskName, string minigameName) = minigame.gameObject.AddComponent<MinigameProperties>().GetCustomTypes();

            if (!string.IsNullOrWhiteSpace(playerTaskName))
            {
                try
                {
                    PlayerTask newTask = task.gameObject.AddInjectedComponentByName(playerTaskName).Cast<PlayerTask>();
                    newTask.StartAt = task.StartAt;
                    newTask.TaskType = task.TaskType;
                    newTask.MinigamePrefab = task.MinigamePrefab;
                    newTask.HasLocation = task.HasLocation;
                    newTask.LocationDirty = task.LocationDirty;
                    newTask.Id = task.Id;
                    newTask.Index = task.Index;

                    DestroyImmediate(task);

                    tasks[i] = newTask.Cast<T>();
                    task = newTask;
                }
                catch (Exception e)
                {
                    Error("Failed to add new minigame");
                    Error(e.ToString());

                    continue;
                }
            }

            if (!string.IsNullOrWhiteSpace(minigameName))
            {
                try
                {
                    Minigame newMinigame = minigame.gameObject.AddInjectedComponentByName(minigameName).Cast<Minigame>();
                    newMinigame.TransType = minigame.TransType;
                    newMinigame.OpenSound = minigame.OpenSound;
                    newMinigame.CloseSound = minigame.CloseSound;

                    task.MinigamePrefab = newMinigame;

                    DestroyImmediate(minigame);
                }
                catch (Exception e)
                {
                    Error("Failed to add new minigame");
                    Error(e.ToString());
                }
            }
        }
    }

    [HideFromIl2Cpp]
    private void ResolveSystemConsoleMinigames(IEnumerable<SystemConsole> consoles)
    {
        List<(SystemConsole console, GameObject minigameObject)> consoleMinigames = [];
        IEnumerable<SystemConsole> filteredConsoles = consoles.Where(c => c.MinigamePrefab && c.MinigamePrefab.GetComponent<DivertPowerMetagame>());
        foreach (SystemConsole console in filteredConsoles) consoleMinigames.Add((console, Instantiate(console.MinigamePrefab.gameObject, referenceHolder)));

        foreach ((_, GameObject minigameObject) in consoleMinigames)
        {
            if (minigameObject.GetComponent<MinigameProperties>()) continue;
            (_, string minigameName) = minigameObject.AddComponent<MinigameProperties>().GetCustomTypes();

            Minigame minigame = minigameObject.GetComponent<Minigame>();

            if (!string.IsNullOrWhiteSpace(minigameName))
            {
                try
                {
                    Minigame newMinigame = minigame.gameObject.AddInjectedComponentByName(minigameName).Cast<Minigame>();
                    newMinigame.TransType = minigame.TransType;
                    newMinigame.OpenSound = minigame.OpenSound;
                    newMinigame.CloseSound = minigame.CloseSound;
                }
                catch (Exception e)
                {
                    Error("Failed to add new minigame");
                    Error(e.ToString());
                }
            }
        }

        foreach ((SystemConsole console, GameObject minigameObject) in consoleMinigames)
        {
            console.MinigamePrefab = minigameObject.GetComponents<Minigame>().First(m => !m.TryCast<DivertPowerMetagame>());
        }
    }

    [HideFromIl2Cpp]
    private void ResolveExileController()
    {
        GameObject exile = Instantiate(minigameProperties.gameObjects[1], referenceHolder);
        SubmergedExileController newExile = exile.AddComponent<SubmergedExileController>();

        StowArms data = exile.transform.GetChild(0).GetComponent<StowArms>();

        newExile.ImpostorText = data.GunContent.GetComponent<TextMeshPro>(); // ImpostorText
        newExile.Text = data.RifleContent.GetComponent<TextMeshPro>(); // MainText
        newExile.TextSound = data.OpenSound; // ExileSound
        newExile.Duration = 6; // Can't serialize this, hardcoded

        normalShip.ExileCutscenePrefab = newExile;
    }

    [HideFromIl2Cpp]
    private void ResolveDoorMinigames()
    {
        List<(DoorConsole console, GameObject minigameObject)> consoleMinigames = [];
        IEnumerable<DoorConsole> filteredConsoles = normalShip.GetComponentsInChildren<DoorConsole>().Where(c => c.MinigamePrefab && c.MinigamePrefab.GetComponent<DivertPowerMetagame>());
        foreach (DoorConsole console in filteredConsoles) consoleMinigames.Add((console, Instantiate(console.MinigamePrefab.gameObject, referenceHolder)));

        foreach ((_, GameObject minigameObject) in consoleMinigames)
        {
            if (minigameObject.GetComponent<MinigameProperties>()) continue;
            (_, string minigameName) = minigameObject.AddComponent<MinigameProperties>().GetCustomTypes();

            Minigame minigame = minigameObject.GetComponent<Minigame>();

            if (!string.IsNullOrWhiteSpace(minigameName))
            {
                try
                {
                    Minigame newMinigame = minigame.gameObject.AddInjectedComponentByName(minigameName).Cast<Minigame>();
                    newMinigame.TransType = minigame.TransType;
                    newMinigame.OpenSound = minigame.OpenSound;
                    newMinigame.CloseSound = minigame.CloseSound;
                }
                catch (Exception e)
                {
                    Error("Failed to add new minigame");
                    Error(e.ToString());
                }
            }
        }

        foreach ((DoorConsole console, GameObject minigameObject) in consoleMinigames)
        {
            console.MinigamePrefab = minigameObject.GetComponents<Minigame>().FirstOrDefault(m => m.TryCast<IDoorMinigame>() != null);
            console.MyDoor = console.GetComponent<PlainDoor>();
        }
    }

    [HideFromIl2Cpp]
    private void ResolveCooldownConsoles(Il2CppReferenceArray<Console> consoles)
    {
        for (int i = 0; i < consoles.Count; i++)
        {
            Console console = consoles[i];
            if (console == null || !console.name.Contains("-CooldownConsole-")) continue;

            CooldownConsole cooldownConsole = console.gameObject.AddComponent<CooldownConsole>();
            cooldownConsole.usableDistance = console.usableDistance;
            cooldownConsole.ConsoleId = console.ConsoleId;
            cooldownConsole.onlyFromBelow = console.onlyFromBelow;
            cooldownConsole.onlySameRoom = console.onlySameRoom;
            cooldownConsole.checkWalls = console.checkWalls;
            cooldownConsole.GhostsIgnored = console.GhostsIgnored;
            cooldownConsole.AllowImpostor = console.AllowImpostor;
            cooldownConsole.Room = console.Room;
            cooldownConsole.TaskTypes = console.TaskTypes;
            cooldownConsole.ValidTasks = console.ValidTasks;
            cooldownConsole.Image = console.Image;

            DestroyImmediate(console);

            consoles[i] = cooldownConsole;
        }
    }

    #endregion

    private static IEnumerable<PlayerTask> GetAllTasks(ShipStatus ship)
        => ship.ShortTasks.Concat(ship.CommonTasks).Concat(ship.LongTasks).Concat(ship.SpecialTasks);
}
