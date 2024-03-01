using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class MinigameProperties(nint ptr) : MonoBehaviour(ptr)
{
    public string @string;
    public AudioClip[] audioClips;
    public Collider2D[] colliders;
    public GameObject[] gameObjects;
    public int[] integers;
    public Sprite[] sprites;
    public Transform[] transforms;
    public Vector2[] vector2S;

    public string playerTaskName = "";
    public string minigameName = "";

    public bool dontCloseOnBgClick;

    public void Awake()
    {
        Transform propObj = transform.Find("MinigameProperties");

        StowArms stowArms = propObj.GetComponent<StowArms>();
        GameSettingMenu gameSettingMenu = propObj.GetComponent<GameSettingMenu>();
        PolishRubyGame polishRubyGame = propObj.GetComponent<PolishRubyGame>();
        TextLink textLink = propObj.GetComponent<TextLink>();
        Tilemap2 tilemap2 = propObj.GetComponent<Tilemap2>();

        @string = textLink.targetUrl;
        audioClips = polishRubyGame.rubSounds;
        colliders = stowArms.GunColliders;
        gameObjects = stowArms.selectorSubobjects;
        integers = polishRubyGame.swipes;
        sprites = tilemap2.sprites;
        transforms = gameSettingMenu.AllItems;
        vector2S = polishRubyGame.directions;

        string[] splits = @string.Split([';'], 2);
        if (splits.Length > 0) playerTaskName = splits[0];
        if (splits.Length > 1) minigameName = splits[1];
    }

    public void CloseTask()
    {
        if (dontCloseOnBgClick) return;

        Minigame[] minigames = GetComponents<Minigame>();
        if (minigames.FirstOrDefault(mg => !mg.TryCast<DivertPowerMetagame>()) is { } m)
            m.Close();
        else
            minigames[0].Close();
    }

    [HideFromIl2Cpp]
    public (string playerTaskName, string minigameName) GetCustomTypes()
    {
        Transform propObj = transform.Find("MinigameProperties");
        TextLink textLink = propObj.GetComponent<TextLink>();

        string p = "", m = "";

        string[] splits = textLink.targetUrl.Split([';'], 2);
        if (splits.Length > 0) p = splits[0];
        if (splits.Length > 1) m = splits[1];

        return (p, m);
    }
}
