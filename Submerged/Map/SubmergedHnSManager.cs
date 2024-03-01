using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Extensions;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Floors;

namespace Submerged.Map;

public static class SubmergedHnSManager
{
    private static int _lastShipStatus = int.MinValue;
    private static bool _lastFloor;

    public static bool CurrentGameIsOnUpperDeck
    {
        get
        {
            int currentInstanceID = ShipStatus.Instance.GetInstanceID();
            if (_lastShipStatus == currentInstanceID) return _lastFloor;
            _lastShipStatus = currentInstanceID;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
            {
                if (!player.Data.Role.IsImpostor) continue;

                FloorHandler floorHandler = FloorHandler.GetFloorHandler(player);
                if (!floorHandler) continue;

                return _lastFloor = floorHandler.onUpper;
            }

            Warning("Could not find impostor!!!");
            return _lastFloor = false;
        }
    }

    public static void ReassignTasks(PlayerControl player, bool forUpper)
    {
        // Warning($"Reassinging tasks for player {player.Data.PlayerName}");

        List<byte> newTasks = [];
        List<CustomTaskTypes> tasksToChooseFrom = CustomTaskTypes.All.Where(t => t.floor ==
                (forUpper ? CustomTaskTypes.Floor.UpperDeck : CustomTaskTypes.Floor.LowerDeck))
            .ToList();

        foreach (GameData.TaskInfo task in player.Data.Tasks)
        {
            NormalPlayerTask originalTask = ShipStatus.Instance.GetTaskById(task.TypeId);
            List<CustomTaskTypes> possibleTasks = tasksToChooseFrom
                .Where(t => t.length == originalTask.Length)
                .ToList();

            // Warning($"Reassinging task {TranslationController.Instance.GetString(originalTask.TaskType)}");

            if (!possibleTasks.Any())
            {
                Warning("Not enough tasks to reassign... Skipping " + originalTask.TaskType);

                continue;
            }

            CustomTaskTypes chosenTask = possibleTasks.Random();
            tasksToChooseFrom.Remove(chosenTask);

            // Warning($"Chose {TranslationController.Instance.GetString(chosenTask.type)} with length {chosenTask.length} as replacement");

            NormalPlayerTask actualTask;

            switch (chosenTask.length)
            {
                case NormalPlayerTask.TaskLength.Common:
                    actualTask = ShipStatus.Instance.CommonTasks.Where(t => t.TaskType == chosenTask.taskType)
                        .Where(AdditionalReplacementChecks)
                        .Random();

                    break;

                case NormalPlayerTask.TaskLength.Long:
                    actualTask = ShipStatus.Instance.LongTasks.Where(t => t.TaskType == chosenTask.taskType)
                        .Where(AdditionalReplacementChecks)
                        .Random();

                    break;

                case NormalPlayerTask.TaskLength.Short:
                    actualTask = ShipStatus.Instance.ShortTasks.Where(t => t.TaskType == chosenTask.taskType)
                        .Where(AdditionalReplacementChecks)
                        .Random();

                    break;

                default:
                    continue;
            }

            if (!actualTask)
            {
                Error($"No actual task when reassigning {chosenTask.taskType} with length {chosenTask.length}");

                continue;
            }

            Warning(TranslationController.Instance.GetString(actualTask.TaskType));
            newTasks.Add((byte) actualTask.Index);
        }

        GameData.Instance.RpcSetTasks(player.PlayerId, newTasks.ToArray());
    }

    private static bool AdditionalReplacementChecks(NormalPlayerTask replacement)
    {
        if (replacement.TaskType != TaskTypes.UploadData) return true;

        // For upload data task, we only want upper deck locations
        return replacement.StartAt is SystemTypes.Admin or SystemTypes.Medical;
    }
}
