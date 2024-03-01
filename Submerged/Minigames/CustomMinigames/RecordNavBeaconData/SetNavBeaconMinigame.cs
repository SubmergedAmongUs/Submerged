using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.RecordNavBeaconData;

[RegisterInIl2Cpp]
public sealed class SetNavBeaconMinigame(nint ptr) : Minigame(ptr)
{
    private static readonly char[] _letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] _numbers = "0123456789".ToCharArray();

    public SetNavBeaconPart1 part1;
    public SetNavBeaconPart2 part2;

    public string code = "";

    private void Start()
    {
        if (MyNormTask.taskStep == 0 && MyNormTask.Data.Length == 0)
        {
            code = "" + _letters[UnityRandom.Range(0, _letters.Length)] + _letters[UnityRandom.Range(0, _letters.Length)] + _numbers[UnityRandom.Range(0, _numbers.Length)];
            MyNormTask.Data = Encoding.ASCII.GetBytes(code);
        }
        else
        {
            code = Encoding.ASCII.GetString(MyNormTask.Data);
        }

        GameObject part1Obj = transform.Find("Part1").gameObject;
        GameObject part2Obj = transform.Find("Part2").gameObject;

        Il2CppArrayBase<TextMeshPro> texts = part1Obj.transform.Find("Code").GetComponentsInChildren<TextMeshPro>();
        Il2CppArrayBase<TextMeshPro> texts2 = part2Obj.transform.Find("Text").GetComponentsInChildren<TextMeshPro>();
        TMP_FontAsset font = HudManager.Instance.IntroPrefab.ImpostorText.font;

        foreach (TextMeshPro textMeshPro in texts)
        {
            textMeshPro.font = font;
        }

        foreach (TextMeshPro textMeshPro in texts2)
        {
            textMeshPro.font = font;
        }

        part1Obj.SetActive(ConsoleId == 0);
        part2Obj.SetActive(ConsoleId == 1);

        part1 = transform.Find("Part1").gameObject.AddComponent<SetNavBeaconPart1>();
        part2 = transform.Find("Part2").gameObject.AddComponent<SetNavBeaconPart2>();
    }
}
