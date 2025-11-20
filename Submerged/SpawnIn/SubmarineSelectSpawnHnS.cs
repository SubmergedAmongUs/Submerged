using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.Floors;
using Submerged.Localization.Strings;
using Submerged.Map;
using Submerged.SpawnIn.Enums;
using Submerged.Systems.SecuritySabotage;
using UnityEngine;

namespace Submerged.SpawnIn;

[RegisterInIl2Cpp]
public sealed class SubmarineSelectSpawnHnS(nint ptr) : SubmarineSelectSpawn(ptr)
{
    private bool _spawned;

    public override void Update()
    {
        bool isImpostor = PlayerControl.LocalPlayer.Data.Role.IsImpostor;

        if (!isImpostor && !clicked)
        {
            clicked = true;
            SetColors(upperDeckRenderers, new Color(1, 1, 1, 0.35f));
            SetColors(lowerDeckRenderers, new Color(1, 1, 1, 0.35f));
            upperDeckText.color = new Color(1, 1, 1, 0.35f);
            lowerDeckText.color = new Color(1, 1, 1, 0.35f);
            this.StartCoroutine(CoWaitForOthers());
        }

        if (!isImpostor && !_spawned && SubmarineSpawnInSystem.Instance.currentState == SpawnInState.Spawning)
        {
            _spawned = true;
            ShipStatus.Instance.RpcUpdateSystem(CustomSystemTypes.SpawnIn, 0);
        }

        switch (SubmarineSpawnInSystem.Instance.currentState)
        {
            case SpawnInState.Loading:
                timerText.text = "";
                playersText.text = string.Format(Tasks.SpawnIn_Bottom_Loading,
                                                 SubmarineSpawnInSystem.Instance.GetReadyPlayerAmount(),
                                                 SubmarineSpawnInSystem.Instance.GetTotalPlayerAmount());

                break;

            case SpawnInState.Spawning when !clicked && SubmarineSpawnInSystem.Instance.timer > 0:
                timerText.text = string.Format(Tasks.SpawnIn_Top_SpawningNotClicked, Mathf.RoundToInt(SubmarineSpawnInSystem.Instance.timer));
                playersText.text = "";

                break;

            case SpawnInState.Spawning when !clicked && SubmarineSpawnInSystem.Instance.timer <= 0:
                timerText.text = "";
                playersText.text = "";
                clicked = true;
                ResetHover();
                this.StartCoroutine(CoSelectLevel(UnityRandom.Range(0, 1f) < 0.5f));

                break;

            case SpawnInState.Spawning when clicked && !isImpostor && SubmarineSpawnInSystem.Instance.timer > 0:
                timerText.text = string.Format(Tasks.SpawnIn_Top_SpawningHnS, Mathf.RoundToInt(SubmarineSpawnInSystem.Instance.timer));
                playersText.text = "";

                break;

            case SpawnInState.Spawning when clicked && isImpostor && SubmarineSpawnInSystem.Instance.timer > 0:
            case SpawnInState.Spawning when clicked && !isImpostor && SubmarineSpawnInSystem.Instance.timer <= 0:
            case SpawnInState.Spawning when clicked && isImpostor && SubmarineSpawnInSystem.Instance.timer <= 0:
            case SpawnInState.Done:
                timerText.text = "";
                playersText.text = "";

                break;
        }

        if (clicked) return;
        controller.Update();

        if (!HudManager.Instance.Chat.IsOpenOrOpening && controller.CheckHover(upperDeckCollider))
            Hover(true);
        else if (!HudManager.Instance.Chat.IsOpenOrOpening && controller.CheckHover(lowerDeckCollider))
            Hover(false);
        else
            ResetHover();
    }

    [HideFromIl2Cpp]
    protected override IEnumerator PlayFinishAnimationWhenWaitingForOthers()
    {
        bool upperSelected = SubmergedHnSManager.CurrentGameIsOnUpperDeck;
        FloorHandler.LocalPlayer.RpcRequestChangeFloor(upperSelected);

        yield return DoPreAnimationLogicWhenFloorClicked();

        yield return PlaySelectAnimation(upperSelected, new Color(1, 1, 1, 0.35f));
        yield return new WaitForSecondsRealtime(1);
        yield return PlayFadeOutAnimation(upperSelected, new Color(1, 1, 1, 0.35f));
    }

    [HideFromIl2Cpp]
    protected override IEnumerator DoPreAnimationLogicWhenFloorClicked()
    {
        bool onUpper = SubmergedHnSManager.CurrentGameIsOnUpperDeck;

        if (AmongUsClient.Instance.AmHost)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                SubmergedHnSManager.ReassignTasks(player, onUpper);
            }

            SubmarineSecuritySabotageSystem.Instance.fixedCams = [0, 1, 2];
            SubmarineSecuritySabotageSystem.Instance.IsDirty = true;
        }

        yield break;
    }
}
