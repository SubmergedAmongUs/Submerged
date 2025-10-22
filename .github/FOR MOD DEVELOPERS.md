# NOTICE

Some information in this document might be outdated, but most of it should be ok.

If you have any doubts check the source code or contact me on discord: `@alexejhero`

---

# For Mod Developers

Submerged tries to patch as little as possible in order to allow other mods to be easily updated to be compatible with it. However, there are a few classes and patches that we have, which will commonly conflict with most other mods.

<br>

# Networking-related IDs used by Submerged

- SpawnablePrefabs
  - `11` - SubmarineStatus
- Systems
    - `130` - SubmarineOxygenSystem
    - `136` - SubmarineElevatorSystem (HallwayLeft)
    - `137` - SubmarineElevatorSystem (HallwayRight)
    - `138` - SubmarineElevatorSystem (LobbyLeft)
    - `139` - SubmarineElevatorSystem (LobbyRight)
    - `140` - SubmarineElevatorSystem (Service)
    - `141` - SubmarinePlayerFloorSystem
    - `142` - SubmarineSecuritySabotageSystem
    - `143` - SubmarineSpawnInSystem
    - `144` - SubmarineBoxCatSystem
- Reactor MethodRPCs
  - `210` - SetCustomData
  - `211` - RequestChangeFloor
  - `213` - EngineVent
  - `214` - OxygenDeath

# Important Classes

## `ElevatorMover`

<u>When playing on Submerged</u>, this component is added to dead bodies and shapeshifter evidences in order to allow them to be moved by elevators.

This system might conflict with mods that change the position of dead bodies (for example, an Undertaker role).

Location: `Submerged.Elevators.Objects.ElevatorMover`

## GenericShadowBehaviour

This component can be added to objects in order to make them cast a shadow from upper deck to lower deck. You might need to create custom shadow renderers to describe how the shadow needs to be drawn. These classes extend `RelativeShadowRenderer`

Location: `Submerged.Floors.Objects.GenericShadowBehaviour`

<br>

# Commonly Conflicting Patches

## `float ShipStatus.CalculateLightRadius(GameData.PlayerInfo player)`

<u>When playing on Submerged</u>, the `float ShipStatus.CalculateLightRadius(GameData.PlayerInfo player)` method is patched to execute `float SubmarineStatus.CalculateLightRadius(GameData.PlayerInfo player)` instead of its default implementation. This method is used by the mod to play the light flicker animation when lights are sabotaged, and to play the sounds as well.

This system **WILL** conflict with any mods that modify the light radius of players.

Location: `Submerged.Map.Patches.ShipStatus_CalculateLightRadius_Patch`

<br>

## `void ArrowBehaviour.UpdatePosition()`

<u>When playing on Submerged</u>, this method is completely overwritten by the mod to make arrows point to the nearest elevator if their target is on the other floor.

This patch may work incorrectly with mods that make use of custom arrow behaviours, since those might not be affected by this.

Location: `Submerged.Minigames.Patches.ArrowBehaviour_Update_Patch`

<br>

## `void Console.Use()`

<u>When playing on Submerged</u>, this method is completely overwritten by the mod to check if the player is trying to open the Fix Wiring task in Electrical, and provide the 8-wires minigame instead of the 4-wires one.

This patch may conflict with mods that patch consoles in order to modify who can use them.

Location: `Submerged.Minigames.CustomMinigames.FixWiring.Patches.Console_Use_Patch`

<br>

## `void Vent.CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)`

<u>When playing on Submerged</u>, this patch ensures that players cannot enter the one-way vent in Engines, and cannot exit the Central vents during the venting transition.

This patch may conflict with mods that patch consoles in order to modify who can use them.

Location: `Submerged.Map.Patches.Vent_CanUse_Patch`

<br>
