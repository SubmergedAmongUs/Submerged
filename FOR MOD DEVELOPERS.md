# For Mod Developers

Submerged tries to patch as little as possible in order to allow other mods to be easily updated to be compatible with it. However, there are a few classes and patches that we have, which will commonly conflict with most other mods.

<br>

# Networking-related IDs used by Submerged

- SpawnablePrefabs
  - `9` - SubmarineStatus
- RPCCalls
  - `210` - SetCustomData
  - `211` - RequestChangeFloor
  - `212` - AcknowledgeChangeFloor
  - `213` - EngineVent
  - `214` - OxygenDeath
- Systems
  - `130` - SubmarineOxygenSystem
  - `136` - SubmarineElevatorSystem (WestLeft)
  - `137` - SubmarineElevatorSystem (WestRight)
  - `138` - SubmarineElevatorSystem (EastLeft)
  - `139` - SubmarineElevatorSystem (EastRight)
  - `140` - SubmarineElevatorSystem (Service)
  - `141` - SubmarinePlayerFloorSystem
  - `142` - SubmarineSecuritySabotageSystem
  - `143` - SubmarineSpawnInSystem
  - `144` - SubmarineBoxCatSystem

# Commonly Conflicting Classes

## `SubmergedDeadBody`

<u>When playing on Submerged</u>, if a player dies their dead body receives a `SubmergedDeadBody` component. This component handles the shadow of the dead body as seen in Lower Central, and moving the dead body between floors if it is in an elevator.

This system might conflict with mods that change the position of dead bodies (for example, an Undertaker role). 

Location: `Submerged.Map.MonoBehaviours.SubmergedDeadBody`

<br>

# Commonly Conflicting Patches

## `float PlatformConsole.CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)`

<u>When playing on Submerged</u>, this method is comptetely overwritten by the mod, as PlatformConsole is used for elevators. 

This patch may conflict with mods that patch consoles in order to modify who can use them.

Location: `Submerged.Systems.CustomSystems.Elevator.Patches.PlatformConsole_CanUse_Patch`

<br>

## `float ShipStatus.CalculateLightRadius(GameData.PlayerInfo player)`

<u>When playing on Submerged</u>, the `float ShipStatus.CalculateLightRadius(GameData.PlayerInfo player)` method is patched to execute `float SubmarineStatus.CalculateLightRadius(GameData.PlayerInfo player)` instead of its default implementation. This method is used by the mod to play the light flicker animation when lights are sabotaged, and to play the sounds as well.

This system **WILL** conflict with any mods that modify the light radius of players.

Location: `Submerged.Map.Patches.ShipStatus_CalculateLightRadius_Patch`

<br>

## `void ArrowBehaviour.Update()`

<u>When playing on Submerged</u>, this method is completely overwritten by the mod to make arrows point to the nearest elevator if their target is on the other floor.

This patch may work incorrectly with mods that make use of custom arrow behaviours, since those might not be affected by this.

Location: `Submerged.Minigames.Patches.ArrowBehaviour_Update_Patch`

<br>

## `void Console.Use()`

<u>When playing on Submerged</u>, this method is completely overwritten by the mod to check if the player is trying to open the Fix Wiring task in Electrical, and provide the 8-wires minigame instead of the 4-wires one.

This patch may conflict with mods that patch consoles in order to modify who can use them.

Location: `Submerged.Minigames.CustomMinigames.FixWiring.Patches.Console_Use_Patch`

<br>

## `void ExileController.Begin(GameData.PlayerInfo exiled, bool tie)`

<u>When playing on Submerged</u>, this method is completely overwritten by the mod. Most of the code is the same, but there are some Submerged-specific calls which are added to the method.

This patch may conflict with mods that patch the exile cutscene in order to show the role name of the person who died.

Location: `Submerged.ExileCutscene.Patches.ExileController_Begin_Patch`

<br>

## `void PlatformConsole.Use()`

<u>When playing on Submerged</u>, this method is comptetely overwritten by the mod, as PlatformConsole is used for elevators. 

This patch may conflict with mods that patch consoles in order to modify who can use them.

Location: `Submerged.Systems.CustomSystems.Elevator.Patches.PlatformConsole_Use_Patch`

<br>

## `void Vent.CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)`

<u>When playing on Submerged</u>, this patch ensures that players cannot enter the one-way vent in Engines, and cannot exit the Central vents during the venting transition.

This patch may conflict with mods that patch consoles in order to modify who can use them.

Location: `Submerged.Map.Patches.Vent_CanUse_Patch`

<br>

## `void Vent.MoveToVent(Vent otherVent)`

<u>When playing on Submerged</u>, this patch handles changing the floor when venting and offsetting the player camera when using either the Admin-Engines vents or the Central vents.

This patch may conflict with mods that have custom actions when venting.

**SUBMERGED AUTOMATICALLY HANDLES ANY CROSS-FLOOR VENTS, EVEN CUSTOM ONES!**

Location: `Submerged.Map.Patches.Vent_MoveToVent_Patch`
